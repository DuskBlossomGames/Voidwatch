using JetBrains.Annotations;
using Player;
using Player.Upgrades;
using Singletons.Static_Info;
using Tutorial;
using Unity.Services.Analytics;

namespace Analytics
{
    public class FirstLaunchEvent : Event
    {
        public FirstLaunchEvent() : base("firstLaunch") { }
    }

    public class CompleteTutorialStageEvent : Event
    {
        public CompleteTutorialStageEvent() : base("completeTutorialStage") { }
        
        public TutorialController.Stage TutorialStage { set => SetParameter("tutorialStage", value.ToString()); }
    }

    public class ChooseUpgradeEvent : Event
    {
        public ChooseUpgradeEvent() : base("chooseUpgrade") { }
        
        public UpgradePlayer.Upgrade Upgrade { set => SetParameter("upgradeName", value.Title); }
    }
    
    public class PilferEvent : Event
    {
        public PilferEvent() : base("pilfer") { }
        
        public int Pilfers { set => SetParameter("pilfers", value.ToString()); }
    }

    public class ScavengeEvent : Event
    {
        public ScavengeEvent() : base("scavenge") { }
    }
    
    public class BoostStatEvent : Event
    {
        public BoostStatEvent() : base("boostStat") { }
        
        public IBoostableStat Boost { set => SetParameter("boostName", value.GetName()); }
        public int Level { set => SetParameter("boostLevels", value); }
    }

    public class HealEvent : Event
    {
        public HealEvent() : base("heal") { }
    }
    
    public class EnterLevelEvent : Event
    {
        public EnterLevelEvent() : base("enterLevel") { }
        
        public LevelType LevelType { set => SetParameter("levelType", value.ToString()); }
        public int LevelNum { set => SetParameter("levelNum", value); }
    }

    public class WinEvent : Event
    {
        public WinEvent() : base("win") { }
        
        public bool HardMode { set =>  SetParameter("hardMode", value); }
    }
    
    public class LoseEvent : Event
    {
        public LoseEvent() : base("lose") { }
        
        public int LevelNum { set => SetParameter("levelNum", value); }
        public bool HardMode { set => SetParameter("hardMode", value); }
        public string DiedTo { set => SetParameter("diedTo", value); }
    }

    public class EditOptionEvent : Event
    {
        public EditOptionEvent() : base("editOption") { }
        
        public string OptionId { set => SetParameter("optionId", value); }
    }
    
    public class VisitScreenEvent : Event
    {
        public VisitScreenEvent() : base("visitScreen") { }
        
        public string ScreenId { set => SetParameter("screenId", value); }
    }
    
    public class PlayEvent : Event
    {
        public PlayEvent() : base("play") { }
        
        public bool HardMode { set =>  SetParameter("hardMode", value); }
    }
    
    public class ZoomMapEvent : Event 
    {
        public ZoomMapEvent() : base("zoomMap") { }
    }
    
    public class PanMapEvent : Event
    {
        public PanMapEvent() : base("panMap") { }
    }
    
    public class ViewLoreEvent : Event
    {
        public ViewLoreEvent() : base("viewLore") { }
    }
}