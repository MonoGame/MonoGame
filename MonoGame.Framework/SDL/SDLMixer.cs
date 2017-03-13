using System;
using System.Runtime.InteropServices;

internal static class SdlMixer
{
    [Flags]
    public enum InitFlag
    {
        Flac,
        Mod,
        Modplug,
        Mp3,
        Ogg,
        Fluidsynth
    }

    private const string NativeLibName = "SDL2_mixer.dll";

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_Init")]
    public static extern int Init(InitFlag flags);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_OpenAudio")]
    public static extern int OpenAudio(int frequency, ushort format, int channels, int chunksize);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_LoadMUS")]
    public static extern IntPtr LoadMUS(string file);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_PlayMusic")]
    public static extern int PlayMusic(IntPtr music, int loops);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_ResumeMusic")]
    public static extern void ResumeMusic();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_PauseMusic")]
    public static extern void PauseMusic();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_FreeMusic")]
    public static extern void FreeMusic(IntPtr music);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_SetMusicPosition")]
    public static extern int SetMusicPosition(double position);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_VolumeMusic")]
    public static extern int VolumeMusic(int volume);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_HookMusicFinished")]
    public static extern void HookMusicFinished(Delegate music_finished);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_CloseAudio")]
    public static extern void CloseAudio();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mix_Quit")]
    public static extern void Quit();
}
