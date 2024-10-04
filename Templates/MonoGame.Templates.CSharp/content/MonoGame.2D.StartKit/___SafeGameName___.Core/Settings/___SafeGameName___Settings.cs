using ___SafeGameName___.Core.Effects;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ___SafeGameName___.Core.Settings;

public class ___SafeGameName___Settings : INotifyPropertyChanged
{
    private bool fullScreen;
    private int language;
    private ParticleEffectType particleEffect;

    public bool FullScreen
    {
        get => fullScreen;
        set
        {
            if (fullScreen != value)
            {
                fullScreen = value;
                OnPropertyChanged();
            }
        }
    }
    public int Language {
        get => language;
        set
        {
            if (language != value)
            {
                language = value;
                OnPropertyChanged();
            }
        }
    }
    public ParticleEffectType ParticleEffect
    {
        get => particleEffect;
        set
        {
            if (particleEffect != value)
            {
                particleEffect = value;
                OnPropertyChanged();
            }
        }
    }

    // Add more settings as needed

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
