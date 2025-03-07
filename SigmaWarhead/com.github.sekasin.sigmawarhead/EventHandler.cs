﻿using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Server = Exiled.Events.Handlers.Server;
using MEC;

namespace SigmaWarhead.com.github.sekasin.sigmawarhead;

public class EventHandler
{
    private readonly Plugin<Config> _main;
    private readonly bool _debugMode;
    private readonly int TimeToLaunch;
    private readonly string LaunchMessage;
    CoroutineHandle timer;

    public EventHandler(Plugin<Config> plugin)
    {
        _main = plugin;
        _debugMode = plugin.Config.Debug;
        if (_debugMode) {
            Log.Info("Loading EventHandler");
        }

        TimeToLaunch = plugin.Config.ActivationTime;
        LaunchMessage = plugin.Config.CassieLines.FirstOrDefault(x => x.Key == "launch").Value;
        
        Server.RoundStarted += StartSigma;
        Server.RestartingRound += StopSigma;
        Server.RoundEnded += StopSigma2;
    }

    public void UnregisterEvents()
    {
        Server.RoundStarted -= StartSigma;
        Server.RestartingRound -= StopSigma;
        Server.RoundEnded -= StopSigma2;
    }

    internal void StartSigma()
    {
        timer = Timing.CallDelayed(TimeToLaunch * 60, LaunchSigmaWarhead);
        if (_debugMode)
        {
            Log.Debug("SigmaWarhead armed and will launch in " + TimeToLaunch + " minutes.");
        }
    }

    internal void StopSigma()
    {
        Timing.KillCoroutines(timer);
        Log.Info("SigmaWarhead timer destroyed.");
    }
    
    internal void StopSigma2(RoundEndedEventArgs args)
    {
        StopSigma();
    }

    internal void LaunchSigmaWarhead()
    {
        Log.Info("SigmaWarhead launched.");
        Cassie.Clear();
        Cassie.Message(LaunchMessage, false, true, true);
        Timing.CallDelayed(15, () => {
            Warhead.Controller.StartDetonation(false, true);
            Warhead.Controller.ForceTime(105);
            Warhead.IsLocked = true;
        });
        StopSigma();
    }
}