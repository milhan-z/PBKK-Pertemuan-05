using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pertemuan_05_WPF_Student_Management.Models;

public class Mahasiswa : INotifyPropertyChanged
{
    private string _nrp = string.Empty;
    private string _nama = string.Empty;
    private string _prodi = string.Empty;
    private int _angkatan;
    private double _ipk;
    private string _status = "Aktif";

    public string Nrp
    {
        get => _nrp;
        set { if (_nrp != value) { _nrp = value; OnPropertyChanged(); } }
    }

    public string Nama
    {
        get => _nama;
        set { if (_nama != value) { _nama = value; OnPropertyChanged(); } }
    }

    public string Prodi
    {
        get => _prodi;
        set { if (_prodi != value) { _prodi = value; OnPropertyChanged(); } }
    }

    public int Angkatan
    {
        get => _angkatan;
        set { if (_angkatan != value) { _angkatan = value; OnPropertyChanged(); } }
    }

    public double Ipk
    {
        get => _ipk;
        set { if (_ipk != value) { _ipk = value; OnPropertyChanged(); } }
    }

    public string Status
    {
        get => _status;
        set { if (_status != value) { _status = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
