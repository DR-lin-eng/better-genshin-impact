using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoCook;
using BetterGenshinImpact.GameTask.AutoDomain;
using BetterGenshinImpact.GameTask.AutoFight;
using BetterGenshinImpact.GameTask.AutoFishing;
using BetterGenshinImpact.GameTask.AutoGeniusInvokation;
using BetterGenshinImpact.GameTask.AutoPick;
using BetterGenshinImpact.GameTask.AutoSkip;
using BetterGenshinImpact.GameTask.AutoWood;
using BetterGenshinImpact.GameTask.AutoMusicGame;
using BetterGenshinImpact.GameTask.QuickTeleport;
using BetterGenshinImpact.Service.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using Fischless.GameCapture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BetterGenshinImpact.GameTask.AutoTrackPath;

namespace BetterGenshinImpact.Core.Config
{
    /// <summary>
    /// 全局配置类 - 集中管理所有功能模块的配置
    /// </summary>
    [Serializable]
    public partial class AllConfig : ObservableObject
    {
        #region 系统设置

        /// <summary>
        /// 窗口捕获模式
        /// </summary>
        [ObservableProperty]
        private string _captureMode = CaptureModes.BitBlt.ToString();

        /// <summary>
        /// 是否启用详细错误日志
        /// </summary>
        [ObservableProperty]
        private bool _detailedErrorLogs;

        /// <summary>
        /// 不展示新版本提示的最新版本号
        /// </summary>
        [ObservableProperty]
        private string _notShowNewVersionNoticeEndVersion = string.Empty;

        /// <summary>
        /// 触发器触发间隔(毫秒)
        /// </summary>
        [ObservableProperty]
        private int _triggerInterval = 50;

        /// <summary>
        /// WGC是否使用位图缓存
        /// 注：高帧率下可能导致卡顿，云原神可能出现黑屏
        /// </summary>
        [ObservableProperty]
        private bool _wgcUseBitmapCache = true;

        /// <summary>
        /// 是否自动修复Win11下BitBlt截图问题
        /// </summary>
        [ObservableProperty]
        private bool _autoFixWin11BitBlt = true;

        /// <summary>
        /// 推理使用的设备类型
        /// </summary>
        [ObservableProperty]
        private string _inferenceDevice = "CPU";

        #endregion

        #region 任务调度

        /// <summary>
        /// 下一个计划任务列表
        /// </summary>
        [ObservableProperty]
        private List<ValueTuple<string, int, string, string>> _nextScheduledTask = [];

        /// <summary>
        /// 已选择的一条龙配置名称
        /// </summary>
        [ObservableProperty]
        private string _selectedOneDragonFlowConfigName = string.Empty;

        #endregion

        #region 功能模块配置

        /// <summary>
        /// 遮罩窗口配置
        /// </summary>
        public MaskWindowConfig MaskWindowConfig { get; set; } = new();

        /// <summary>
        /// 通用配置
        /// </summary>
        public CommonConfig CommonConfig { get; set; } = new();

        /// <summary>
        /// 原神启动配置
        /// </summary>
        public GenshinStartConfig GenshinStartConfig { get; set; } = new();

        /// <summary>
        /// 自动拾取配置
        /// </summary>
        public AutoPickConfig AutoPickConfig { get; set; } = new();

        /// <summary>
        /// 自动剧情配置
        /// </summary>
        public AutoSkipConfig AutoSkipConfig { get; set; } = new();

        /// <summary>
        /// 自动钓鱼配置
        /// </summary>
        public AutoFishingConfig AutoFishingConfig { get; set; } = new();

        /// <summary>
        /// 快速传送配置
        /// </summary>
        public QuickTeleportConfig QuickTeleportConfig { get; set; } = new();

        /// <summary>
        /// 自动烹饪配置
        /// </summary>
        public AutoCookConfig AutoCookConfig { get; set; } = new();

        /// <summary>
        /// 七圣召唤配置
        /// </summary>
        public AutoGeniusInvokationConfig AutoGeniusInvokationConfig { get; set; } = new();

        /// <summary>
        /// 自动伐木配置
        /// </summary>
        public AutoWoodConfig AutoWoodConfig { get; set; } = new();

        /// <summary>
        /// 自动战斗配置
        /// </summary>
        public AutoFightConfig AutoFightConfig { get; set; } = new();

        /// <summary>
        /// 自动音乐会配置
        /// </summary>
        public AutoMusicGameConfig AutoMusicGameConfig { get; set; } = new();

        /// <summary>
        /// 自动秘境配置
        /// </summary>
        public AutoDomainConfig AutoDomainConfig { get; set; } = new();

        /// <summary>
        /// 宏配置
        /// </summary>
        public MacroConfig MacroConfig { get; set; } = new();

        /// <summary>
        /// 录制配置
        /// </summary>
        public RecordConfig RecordConfig { get; set; } = new();

        /// <summary>
        /// 脚本配置
        /// </summary>
        public ScriptConfig ScriptConfig { get; set; } = new();

        /// <summary>
        /// 路径追踪配置
        /// </summary>
        public PathingConditionConfig PathingConditionConfig { get; set; } = PathingConditionConfig.Default;

        #endregion

        #region 界面配置

        /// <summary>
        /// 快捷键配置
        /// </summary>
        public HotKeyConfig HotKeyConfig { get; set; } = new();

        /// <summary>
        /// 通知配置
        /// </summary>
        public NotificationConfig NotificationConfig { get; set; } = new();

        /// <summary>
        /// 按键绑定配置
        /// </summary>
        public KeyBindingsConfig KeyBindingsConfig { get; set; } = new();

        /// <summary>
        /// 传送相关配置
        /// </summary>
        public TpConfig TpConfig { get; set; } = new();

        #endregion

        #region 事件处理

        /// <summary>
        /// 任意配置变更时的回调
        /// </summary>
        [JsonIgnore]
        public Action? OnAnyChangedAction { get; set; }

        /// <summary>
        /// 初始化事件
        /// </summary>
        public void InitEvent()
        {
            // 监听本类的属性变更
            PropertyChanged += OnAnyPropertyChanged;

            // 监听各模块配置的变更
            MaskWindowConfig.PropertyChanged += OnAnyPropertyChanged;
            CommonConfig.PropertyChanged += OnAnyPropertyChanged;
            GenshinStartConfig.PropertyChanged += OnAnyPropertyChanged;
            NotificationConfig.PropertyChanged += OnAnyPropertyChanged;
            NotificationConfig.PropertyChanged += OnNotificationPropertyChanged;
            KeyBindingsConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoPickConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoSkipConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoFishingConfig.PropertyChanged += OnAnyPropertyChanged;
            QuickTeleportConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoCookConfig.PropertyChanged += OnAnyPropertyChanged;
            MacroConfig.PropertyChanged += OnAnyPropertyChanged;
            HotKeyConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoWoodConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoFightConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoDomainConfig.PropertyChanged += OnAnyPropertyChanged;
            AutoMusicGameConfig.PropertyChanged += OnAnyPropertyChanged;
            TpConfig.PropertyChanged += OnAnyPropertyChanged;
            ScriptConfig.PropertyChanged += OnAnyPropertyChanged;
            PathingConditionConfig.PropertyChanged += OnAnyPropertyChanged;
        }

        /// <summary>
        /// 处理任意属性变更事件
        /// </summary>
        public void OnAnyPropertyChanged(object? sender, EventArgs args)
        {
            // 刷新触发器配置
            GameTaskManager.RefreshTriggerConfigs();
            // 执行配置变更回调
            OnAnyChangedAction?.Invoke();
        }

        /// <summary>
        /// 处理通知配置变更事件
        /// </summary>
        public void OnNotificationPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            // 刷新通知器
            NotificationService.Instance().RefreshNotifiers();
        }

        #endregion
    }
}
