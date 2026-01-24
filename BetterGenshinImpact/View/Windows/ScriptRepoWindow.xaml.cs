using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Script;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.Helpers;
using BetterGenshinImpact.Service;
using BetterGenshinImpact.Service.Interface;
using Wpf.Ui.Violeta.Controls;

namespace BetterGenshinImpact.View.Windows;

[ObservableObject]
public partial class ScriptRepoWindow
{
    private const string CustomChannelKey = "custom";

    // Update channel class
    public class RepoChannel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public RepoChannel(string key, string name, string url)
        {
            Key = key;
            Name = name;
            Url = url;
        }
    }

    // Channel list
    private ObservableCollection<RepoChannel> _repoChannels;
    public ObservableCollection<RepoChannel> RepoChannels => _repoChannels;

    // Selected channel
    [ObservableProperty] private RepoChannel? _selectedRepoChannel;

    // Control whether repository address is read-only
    [ObservableProperty] private bool _isRepoUrlReadOnly = true;

    // Repository URL to display/edit
    [ObservableProperty] private string _repoUrl = string.Empty;

    // Progress-related observable properties
    [ObservableProperty] private bool _isUpdating;
    [ObservableProperty] private int _updateProgressValue;
    [ObservableProperty] private string _updateProgressText = "准备更新...";
    [ObservableProperty] private ScriptConfig _config = TaskContext.Instance().Config.ScriptConfig;

    public ScriptRepoWindow()
    {
        InitializeRepoChannels();
        InitializeComponent();
        DataContext = this;
        Config.PropertyChanged += OnConfigPropertyChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //OnSelectedRepoChannelChanged
        if (e.PropertyName == nameof(SelectedRepoChannel))
        {
            OnSelectedRepoChannelChanged();
        }
    }

    ~ScriptRepoWindow()
    {
        Config.PropertyChanged -= OnConfigPropertyChanged;
        PropertyChanged -= OnPropertyChanged;
    }

    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScriptConfig.SelectedChannelName))
        {
            OnConfigSelectedChannelChanged();
        }
        else if (e.PropertyName == nameof(ScriptConfig.CustomRepoUrl))
        {
            if (IsCustomChannel(SelectedRepoChannel))
            {
                RepoUrl = Config.CustomRepoUrl;
            }
        }
    }

    private void InitializeRepoChannels()
    {
        var localizationService = App.GetService<ILocalizationService>();
        var customText = localizationService.GetString("scriptRepo.custom");
        var customUrl = string.IsNullOrWhiteSpace(Config.CustomRepoUrl)
            ? "https://example.com/custom-repo"
            : Config.CustomRepoUrl;

        _repoChannels = new ObservableCollection<RepoChannel>
        {
            new("CNB", "CNB", "https://cnb.cool/bettergi/bettergi-scripts-list"),
            new("GitCode", "GitCode", "https://gitcode.com/huiyadanli/bettergi-scripts-list"),
            // Currently unavailable
            // new("Gitee", "https://gitee.com/babalae/bettergi-scripts-list"),
            new("GitHub", "GitHub", "https://github.com/babalae/bettergi-scripts-list"),
            new(CustomChannelKey, customText, customUrl)
        };

        if (string.IsNullOrEmpty(Config.SelectedChannelName))
        {
            // Select the first channel by default
            SelectedRepoChannel = _repoChannels[0];
            Config.SelectedChannelName = SelectedRepoChannel.Key;
        }
        else
        {
            // Try to find the corresponding channel based on the URL in the configuration
            OnConfigSelectedChannelChanged();
        }

        if (SelectedRepoChannel != null)
        {
            OnSelectedRepoChannelChanged();
        }
    }

    // Config.SelectedChannelName change
    private void OnConfigSelectedChannelChanged()
    {
        // If the channel in the configuration doesn't match the currently selected channel, update the selected channel
        if (SelectedRepoChannel?.Key == Config.SelectedChannelName)
        {
            return;
        }

        SelectedRepoChannel = _repoChannels.FirstOrDefault(c => c.Key == Config.SelectedChannelName) ?? _repoChannels[0];
    }

    private void OnSelectedRepoChannelChanged()
    {
        if (SelectedRepoChannel is null)
        {
            return;
        }

        // Update repository address read-only status
        IsRepoUrlReadOnly = !IsCustomChannel(SelectedRepoChannel);

        if (IsCustomChannel(SelectedRepoChannel))
        {
            if (string.IsNullOrWhiteSpace(Config.CustomRepoUrl))
            {
                Config.CustomRepoUrl = SelectedRepoChannel.Url;
            }
            RepoUrl = Config.CustomRepoUrl;
        }
        else
        {
            RepoUrl = SelectedRepoChannel.Url;
        }

        // Update selected repository channel in configuration
        if (Config.SelectedChannelName != SelectedRepoChannel.Key)
        {
            Config.SelectedChannelName = SelectedRepoChannel.Key;
        }
    }

    partial void OnRepoUrlChanged(string value)
    {
        if (!IsCustomChannel(SelectedRepoChannel))
        {
            return;
        }

        Config.CustomRepoUrl = value;
        SelectedRepoChannel!.Url = value;
    }

    private static bool IsCustomChannel(RepoChannel? channel)
    {
        return channel?.Key == CustomChannelKey;
    }

    [RelayCommand]
    private async Task UpdateRepo()
    {
        var localizationService = App.GetService<ILocalizationService>();
        
        if (SelectedRepoChannel is null)
        {
            Toast.Warning(localizationService.GetString("toast.selectRepoChannel"));
            return;
        }
        try
        {
            // Use the selected channel's URL for update
            string repoUrl = RepoUrl;

            // Show updating notification
            Toast.Information(localizationService.GetString("toast.updatingRepo"));

            // Set progress display
            IsUpdating = true;
            UpdateProgressValue = 0;
            UpdateProgressText = App.GetService<ILocalizationService>().GetString("scriptRepo.preparingUpdate");
            // Execute update (repoPath, updated)
            var (_, updated) = await ScriptRepoUpdater.Instance.UpdateCenterRepoByGit(repoUrl,
                (path, steps, totalSteps) =>
                {
                    // Update progress display
                    double progressPercentage = totalSteps > 0 ? Math.Min(100, (double)steps / totalSteps * 100) : 0;
                    UpdateProgressValue = (int)progressPercentage;
                    UpdateProgressText = $"{path}";
                });


            // Update result notification
            if (updated)
            {
                Toast.Success(localizationService.GetString("toast.repoUpdateSuccess"));
            }
            else
            {
                Toast.Success(localizationService.GetString("toast.repoAlreadyLatest"));
            }
        }
        catch (Exception ex)
        {
            await MessageBox.ErrorAsync(localizationService.GetString("dialog.updateFailed", ex.Message));
        }
        finally
        {
            // Hide progress bar
            IsUpdating = false;
        }
    }

    [RelayCommand]
    private void OpenLocalScriptRepo()
    {
        TaskContext.Instance().Config.ScriptConfig.ScriptRepoHintDotVisible = false;
        ScriptRepoUpdater.Instance.OpenLocalRepoInWebView();
        Close();
    }

    [RelayCommand]
    private async Task ResetRepo()
    {
        var localizationService = App.GetService<ILocalizationService>();
        
        if (IsUpdating)
        {
            Toast.Warning(localizationService.GetString("toast.waitForUpdateComplete"));
            return;
        }

        // Add confirmation dialog
        var result = await MessageBox.ShowAsync(
            localizationService.GetString("dialog.confirmResetRepo"),
            localizationService.GetString("dialog.confirmReset"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                if (Directory.Exists(ScriptRepoUpdater.CenterRepoPath))
                {
                    DirectoryHelper.DeleteReadOnlyDirectory(ScriptRepoUpdater.CenterRepoPath);
                    Toast.Success(localizationService.GetString("toast.repoResetSuccess"));
                }
                else
                {
                    Toast.Information(localizationService.GetString("toast.repoNotExist"));
                }
            }
            catch (Exception ex)
            {
                Toast.Error(localizationService.GetString("toast.resetFailed", ex.Message));
            }
        }
    }
}
