using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DES SONS ET MUSIQUES
public class AudioManager : IDisposable
{
    public delegate void BeatEventHandler();
    public event BeatEventHandler OnBeat;
    public delegate void QuarterBeatEventHandler(int pQuarterCount);
    public event QuarterBeatEventHandler OnQuarterBeat;
    public delegate void ThirdOfBeatEventHandler(int pThirdCount);
    public event ThirdOfBeatEventHandler OnThirdOfBeat;

    public float MainTempo { get; private set; }
    public float MainVolume = 1f;
    public float MusicVolume = 1f;
    public float FXVolume = 1f;


    float _BPMTempo = 100;
    Music _mainTheme;

    int _timer;
    float _quarterTimer;
    int _quarterCounter;
    float _thirdTimer;
    int _thirdCounter;

    bool _isMusicSet;
    string _introMusicPath;
    string _loopMusicPath;

    Dictionary<Sound, Action> _soundsWithCallBack = new();
    Dictionary<IListenable, (Music, float)> _loopingSound = new();

    public AudioManager()
    {
        //Initialisation  des données du tempo en nombre de frame, en fonction du 
        MainTempo = 60 / _BPMTempo * ServiceLocator.Get<GameParameters>().TargetFPS;

        if (!IsAudioDeviceReady()) InitAudioDevice();
    }

    public void Update()
    {
        if (_isMusicSet)
        {
            if (IsMusicStreamPlaying(_mainTheme))
            {
                //Maj du stream musical
                UpdateMusicStream(_mainTheme);

                //Maj des timers
                _timer--;
                _quarterTimer--;
                _thirdTimer--;
                if (_timer <= 0)
                {
                    _timer = (int)MainTempo;
                    OnBeat?.Invoke();
                }
                if (_quarterTimer <= 0)
                {
                    _quarterCounter++;
                    if (_quarterCounter >= 4) _quarterCounter = 0;

                    //+= pour conserver l'éventuelle partie décimale négative (si tempo pas multiple de 4)
                    _quarterTimer += MainTempo / 4;
                    OnQuarterBeat?.Invoke(_quarterCounter);
                }
                if (_thirdTimer <= 0)
                {
                    _thirdCounter++;
                    if (_thirdCounter >= 3) _thirdCounter = 0;

                    //+= pour conserver l'éventuelle partie décimale négative (si tempo pas multiple de 4)
                    _thirdTimer += MainTempo / 3;
                    OnThirdOfBeat?.Invoke(_thirdCounter);
                }
            }
            else if (!_mainTheme.Looping) LoadMusicLoop();
        }

        if (_soundsWithCallBack.Count > 0)
            foreach (KeyValuePair<Sound, Action> lSound in _soundsWithCallBack)
                if (!IsSoundPlaying(lSound.Key))
                {
                    lSound.Value();
                    _soundsWithCallBack.Remove(lSound.Key);
                }
        if (_loopingSound.Count > 0)
            foreach (KeyValuePair<IListenable, (Music, float)> lSound in _loopingSound)
            {
                SetMusicVolume(lSound.Value.Item1, GetVolumeFromDistance(lSound.Value.Item2, lSound.Key.Position) * MainVolume*FXVolume);
                UpdateMusicStream(lSound.Value.Item1);
            }
    }

    //Déclare une musique d'intro et une boucle principale
    public void SetIntroAndLoopMusic(string pIntroMusicPath, string pLoopMusicPath)
    {
        _introMusicPath = pIntroMusicPath;
        _loopMusicPath = pLoopMusicPath;
        _isMusicSet = true;

        LoadMusicIntro();
    }
    //Déclare une boucle principale
    public void SetLoopMusic(string pMusicPath)
    {
        _loopMusicPath = pMusicPath;
        _isMusicSet = true;

        LoadMusicLoop();
    }

    //lancement de la musique
    private void LoadMusicIntro()
    {
        _mainTheme = LoadMusicStream(_introMusicPath);
        _mainTheme.Looping = false;
        SetMusicVolume(_mainTheme, MainVolume*MusicVolume);
        PlayMusicStream(_mainTheme);

        //On lance les timers 
        StartTimers();
    }

    //Passe de l'intro de la musique à la boucle principale
    private void LoadMusicLoop()
    {
        UnloadMusicStream(_mainTheme);
        _mainTheme.Looping = true;
        _mainTheme = LoadMusicStream(_loopMusicPath);
        SetMusicVolume(_mainTheme, MainVolume*MusicVolume);
        PlayMusicStream(_mainTheme);

        StartTimers();
    }

    //Initialisation des différents timers
    private void StartTimers()
    {
        _timer = (int)MainTempo;
        OnBeat?.Invoke();
        _quarterTimer = MainTempo / 4;
        _quarterCounter = 0;
        OnQuarterBeat?.Invoke(_quarterCounter);
        _thirdTimer = MainTempo / 3;
        _thirdCounter = 0;
        OnThirdOfBeat?.Invoke(_thirdCounter);
    }

    public void StartMusic()
    {
        SetMusicVolume(_mainTheme, MainVolume*MusicVolume);
        PlayMusicStream(_mainTheme);
    }

    public void PauseMusic()
    {
        PauseMusicStream(_mainTheme);
    }

    public void StopMusic()
    {
        StopMusicStream(_mainTheme);
        UnloadMusicStream(_mainTheme);
        _mainTheme = new();
    }

    //Joue un Fx sonore
    public void PlaySoundFx(Sound pSound, float pVolume, Vector2 pPosition, Action pEndCallBack)
    {
        pVolume = GetVolumeFromDistance(pVolume, pPosition);
        PlaySoundFx(pSound, pVolume, pEndCallBack);
    }
    public void PlaySoundFx(Sound[] pSounds, float pVolume, Vector2 pPosition, Action pEndCallBack)
    {
        pVolume = GetVolumeFromDistance(pVolume, pPosition);
        PlaySoundFx(pSounds[new Random().Next(0, pSounds.Length - 1)], pVolume, pEndCallBack);
    }
    public void PlaySoundFx(Sound pSound, float pVolume, Vector2 pPosition)
    {
        pVolume = GetVolumeFromDistance(pVolume, pPosition);
        PlaySoundFx(pSound, pVolume);
    }
    public void PlaySoundFx(Sound[] pSounds, float pVolume, Vector2 pPosition)
    {
        pVolume = GetVolumeFromDistance(pVolume, pPosition);
        PlaySoundFx(pSounds[new Random().Next(0, pSounds.Length - 1)], pVolume);
    }
    public void PlaySoundFx(Sound[] pSounds, float pVolume = 1)
    {
        PlaySoundFx(pSounds[new Random().Next(0, pSounds.Length - 1)], pVolume);
    }
    public void PlaySoundFx(Sound pSound, float pVolume = 1, Action pEndCallBack = null)
    {
        Sound vSound = LoadSoundAlias(pSound);
        SetSoundVolume(vSound, pVolume * MainVolume*FXVolume);
        PlaySound(vSound);

        if (pEndCallBack != null)
            _soundsWithCallBack.Add(vSound, pEndCallBack);
    }

    public void PlayLoopSoundFx(Music pLoopSound, IListenable pPositionnable, float pVolumeInit)
    {
        _loopingSound.Add(pPositionnable, (pLoopSound, pVolumeInit));
        PlayMusicStream(pLoopSound);
    }

    public void RemoveLoopSoundFx(IListenable pPositionnable)
    {
        if (!_loopingSound.ContainsKey(pPositionnable)) return;

        UnloadMusicStream(_loopingSound[pPositionnable].Item1);
        _loopingSound.Remove(pPositionnable);
    }

    public void Dispose()
    {
        if (IsMusicValid(_mainTheme)) StopMusic();

        _soundsWithCallBack.Clear();

        foreach (var lSound in _loopingSound) UnloadMusicStream(lSound.Value.Item1);
        _loopingSound.Clear();

        ResetSubscriptions();

        _quarterCounter = 0;
        _isMusicSet = false;

    }

    public void ChangeMusicVolume(float pVolume)
    {
        MusicVolume = pVolume;
        if (IsMusicValid(_mainTheme)) SetMusicVolume(_mainTheme, pVolume*MainVolume);
    }

    float GetVolumeFromDistance(float pVolume, Vector2 pPosition)
    {
        float vNewVolume;
        float vMaxDistance = 500;
        float vClampedDist = Math.Clamp(Vector2.Distance(pPosition, ServiceLocator.Get<MyCamera>().Target), 0, vMaxDistance);
        float vRatioOnDistance = Math.Clamp(1 - vClampedDist / vMaxDistance, 0, 1);
        vNewVolume = Tools.InOutQuad(vRatioOnDistance) * pVolume;

        return vNewVolume;
    }

    private void ResetSubscriptions()
    {
        OnBeat = null;
        OnQuarterBeat = null;
    }
}

//ENSEMBLE DES FXs SONORES
public static class FXSounds
{
    private static Sound? _clickSound;
    public static Sound ClickSound
    {
        get
        {
            if (_clickSound == null) _clickSound = LoadSound("audio/buttonPress.mp3");
            return (Sound)_clickSound;
        }
    }

    private static Sound? _fallSound;
    public static Sound FallSound
    {
        get
        {
            if (_fallSound == null) _fallSound = LoadSound("audio/playerFall.mp3");
            return (Sound)_fallSound;
        }
    }

    private static Sound? _crashSound;
    public static Sound CrashSound
    {
        get
        {
            if (_crashSound == null) _crashSound = LoadSound("audio/playerCrash.mp3");
            return (Sound)_crashSound;
        }
    }
    private static Sound? _barrierExplodeSound;
    public static Sound BarrierExplodeSound
    {
        get
        {
            if (_barrierExplodeSound == null) _barrierExplodeSound = LoadSound("audio/barrierExplode.mp3");
            return (Sound)_barrierExplodeSound;
        }
    }
    private static Sound? _orbExplodeSound;
    public static Sound OrbExplodeSound
    {
        get
        {
            if (_orbExplodeSound == null) _orbExplodeSound = LoadSound("audio/orbExplode.mp3");
            return (Sound)_orbExplodeSound;
        }
    }
    private static Sound? _playerBreakSound;
    public static Sound PlayerBreakSound
    {
        get
        {
            if (_playerBreakSound == null) _playerBreakSound = LoadSound("audio/playerBreak.mp3");
            return (Sound)_playerBreakSound;
        }
    }

    private static Sound? _winJingleSound;
    public static Sound WinJingleSound
    {
        get
        {
            if (_winJingleSound == null) _winJingleSound = LoadSound("audio/winJingle.mp3");
            return (Sound)_winJingleSound;
        }
    }

    private static Sound[] _flameBurstSounds;
    public static Sound[] FlameBurstSounds
    {
        get
        {
            if (_flameBurstSounds == null)
            {
                _flameBurstSounds = [LoadSound("audio/flameBurst1.mp3"),
                LoadSound("audio/flameBurst2.mp3"),
                LoadSound("audio/flameBurst3.mp3"),
                LoadSound("audio/flameBurst4.mp3"),
                LoadSound("audio/flameBurst5.mp3")  ];
            }
            return _flameBurstSounds;
        }
    }

    private static Sound[] _laserSounds;
    public static Sound[] LaserSounds
    {
        get
        {
            if (_laserSounds == null)
            {
                _laserSounds = [LoadSound("audio/laser1.mp3"),
                LoadSound("audio/laser2.mp3"),
                LoadSound("audio/laser3.mp3"),
                LoadSound("audio/laser4.mp3"),
                LoadSound("audio/laser5.mp3")  ];
            }
            return _laserSounds;
        }
    }

    private static Sound[] _obstacleBreakSounds;
    public static Sound[] ObstacleBreakSounds
    {
        get
        {
            if (_obstacleBreakSounds == null)
            {
                _obstacleBreakSounds = [LoadSound("audio/obstacle_break1.mp3"),
                LoadSound("audio/obstacle_break2.mp3"),
                LoadSound("audio/obstacle_break3.mp3")  ];
            }
            return _obstacleBreakSounds;
        }
    }

    private static Sound[] _scissorCutSounds;
    public static Sound[] ScissorCutSounds
    {
        get
        {
            if (_scissorCutSounds == null)
            {
                _scissorCutSounds = [LoadSound("audio/scissor_cut1.mp3"),
                LoadSound("audio/scissor_cut2.mp3"),
                LoadSound("audio/scissor_cut3.mp3"),
                LoadSound("audio/scissor_cut4.mp3"),
                LoadSound("audio/scissor_cut5.mp3")  ];
            }
            return _scissorCutSounds;
        }
    }


    private static Sound[] _bigCrackSounds;
    public static Sound[] BigCrackSounds
    {
        get
        {
            if (_bigCrackSounds == null)
            {
                _bigCrackSounds = [LoadSound("audio/bigCrack1.mp3"),
                LoadSound("audio/bigCrack2.mp3"),
                LoadSound("audio/bigCrack3.mp3")  ];
            }
            return _bigCrackSounds;
        }
    }

    private static Sound[] _littleCrackSounds;
    public static Sound[] littleCrackSounds
    {
        get
        {
            if (_littleCrackSounds == null)
            {
                _littleCrackSounds = [LoadSound("audio/littleCrack1.mp3"),
                LoadSound("audio/littleCrack2.mp3"),
                LoadSound("audio/littleCrack3.mp3")  ];
            }
            return _littleCrackSounds;
        }
    }

    public static Music WhirlSound
    {
        get
        {
            return LoadMusicStream("audio/whirl.mp3");
        }
    }

    public static Music BarrierSwooshSound
    {
        get
        {
            return LoadMusicStream("audio/barrierSwoosh.mp3");
        }
    }


}