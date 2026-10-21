using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pertemuan_05_WPF_Student_Management.Models;

namespace Pertemuan_05_WPF_Student_Management;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Mahasiswa> _listMahasiswa;
    private readonly ICollectionView _mahasiswaView;
    private Mahasiswa? _selectedMahasiswa;

    public MainWindow()
    {
        InitializeComponent();

        // Inisialisasi Koleksi Data Mahasiswa (In-Memory)
        _listMahasiswa = new ObservableCollection<Mahasiswa>
        {
            new Mahasiswa { Nrp = "5025241264", Nama = "Muhammad Hilman Azhar", Prodi = "Teknik Informatika", Angkatan = 2024, Ipk = 3.88, Status = "Aktif" },
            new Mahasiswa { Nrp = "5025241010", Nama = "Ahmad Fauzi Rahman", Prodi = "Teknik Informatika", Angkatan = 2024, Ipk = 3.75, Status = "Aktif" },
            new Mahasiswa { Nrp = "5025231045", Nama = "Siti Nurhaliza", Prodi = "Sistem Informasi", Angkatan = 2023, Ipk = 3.90, Status = "Aktif" },
            new Mahasiswa { Nrp = "5025221088", Nama = "Budi Santoso", Prodi = "Teknologi Informasi", Angkatan = 2022, Ipk = 3.45, Status = "Cuti" },
            new Mahasiswa { Nrp = "5025241120", Nama = "Dewi Anggraini", Prodi = "Sains Data", Angkatan = 2024, Ipk = 3.82, Status = "Aktif" }
        };

        _mahasiswaView = CollectionViewSource.GetDefaultView(_listMahasiswa);
        _mahasiswaView.Filter = FilterMahasiswa;

        dgMahasiswa.ItemsSource = _mahasiswaView;

        PerbaruiStatistik();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("--screenshot"))
        {
            // Pilih mahasiswa pertama untuk memperlihatkan fitur seleksi & tombol edit
            if (_listMahasiswa.Count > 0)
            {
                dgMahasiswa.SelectedIndex = 0;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(500);
                AmbilScreenshot();
                Application.Current.Shutdown();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }

    private void PerbaruiStatistik()
    {
        int total = _listMahasiswa.Count;
        double rataIpk = total > 0 ? _listMahasiswa.Average(m => m.Ipk) : 0.0;
        int aktif = _listMahasiswa.Count(m => m.Status == "Aktif");
        int cuti = _listMahasiswa.Count(m => m.Status == "Cuti");

        lblTotalMahasiswa.Text = total.ToString();
        lblRataIpk.Text = rataIpk.ToString("F2", CultureInfo.InvariantCulture);
        lblStatusAktifCuti.Text = $"{aktif} Aktif / {cuti} Cuti";
    }

    private bool FilterMahasiswa(object obj)
    {
        if (obj is not Mahasiswa m) return false;
        string keyword = txtCari.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(keyword)) return true;

        return m.Nrp.ToLower().Contains(keyword) ||
               m.Nama.ToLower().Contains(keyword) ||
               m.Prodi.ToLower().Contains(keyword);
    }

    private void txtCari_TextChanged(object sender, TextChangedEventArgs e)
    {
        _mahasiswaView.Refresh();
    }

    private void dgMahasiswa_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgMahasiswa.SelectedItem is Mahasiswa m)
        {
            _selectedMahasiswa = m;

            txtNrp.Text = m.Nrp;
            txtNama.Text = m.Nama;
            txtIpk.Text = m.Ipk.ToString("F2", CultureInfo.InvariantCulture);

            // Set ComboBox Prodi
            foreach (ComboBoxItem item in cmbProdi.Items)
            {
                if (item.Content.ToString() == m.Prodi)
                {
                    cmbProdi.SelectedItem = item;
                    break;
                }
            }

            // Set ComboBox Angkatan
            foreach (ComboBoxItem item in cmbAngkatan.Items)
            {
                if (item.Content.ToString() == m.Angkatan.ToString())
                {
                    cmbAngkatan.SelectedItem = item;
                    break;
                }
            }

            // Set ComboBox Status
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == m.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }

            // Kunci NRP agar tidak diubah sembarangan saat mode edit
            txtNrp.IsReadOnly = true;
            txtNrp.Background = new SolidColorBrush(Color.FromRgb(241, 245, 249));

            // Perbarui Mode Form & Tombol
            badgeFormMode.Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)); // #FEF3C7
            badgeFormMode.BorderBrush = new SolidColorBrush(Color.FromRgb(252, 211, 77)); // #FCD34D
            txtFormMode.Foreground = new SolidColorBrush(Color.FromRgb(180, 83, 9)); // #B45309
            txtFormMode.Text = $"Mode: Ubah / Hapus Data Mahasiswa ({m.Nrp})";

            btnTambah.IsEnabled = false;
            btnUpdate.IsEnabled = true;
            btnHapus.IsEnabled = true;
        }
    }

    private void btnTambah_Click(object sender, RoutedEventArgs e)
    {
        string nrp = txtNrp.Text.Trim();
        string nama = txtNama.Text.Trim();
        string ipkStr = txtIpk.Text.Trim().Replace(',', '.');

        if (string.IsNullOrWhiteSpace(nrp) || !long.TryParse(nrp, out _))
        {
            MessageBox.Show("NRP wajib berupa deretan angka!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNrp.Focus();
            return;
        }

        if (_listMahasiswa.Any(m => m.Nrp == nrp))
        {
            MessageBox.Show($"Mahasiswa dengan NRP {nrp} sudah terdaftar di sistem!", "Duplikasi NRP", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNrp.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(nama) || nama.Length < 3)
        {
            MessageBox.Show("Nama lengkap minimal 3 karakter!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNama.Focus();
            return;
        }

        if (!double.TryParse(ipkStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double ipk) || ipk < 0.0 || ipk > 4.0)
        {
            MessageBox.Show("IPK harus berupa angka antara 0.00 hingga 4.00!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtIpk.Focus();
            return;
        }

        string prodi = (cmbProdi.SelectedItem is ComboBoxItem p) ? p.Content.ToString() ?? "" : "";
        int angkatan = (cmbAngkatan.SelectedItem is ComboBoxItem a && int.TryParse(a.Content.ToString(), out int angk)) ? angk : 2024;
        string status = (cmbStatus.SelectedItem is ComboBoxItem s) ? s.Content.ToString() ?? "Aktif" : "Aktif";

        Mahasiswa mhsBaru = new Mahasiswa
        {
            Nrp = nrp,
            Nama = nama,
            Prodi = prodi,
            Angkatan = angkatan,
            Ipk = ipk,
            Status = status
        };

        _listMahasiswa.Add(mhsBaru);
        PerbaruiStatistik();
        ResetForm();

        MessageBox.Show($"Mahasiswa baru {nama} ({nrp}) berhasil ditambahkan!", "Sukses Tambah Data", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMahasiswa == null) return;

        string nama = txtNama.Text.Trim();
        string ipkStr = txtIpk.Text.Trim().Replace(',', '.');

        if (string.IsNullOrWhiteSpace(nama) || nama.Length < 3)
        {
            MessageBox.Show("Nama lengkap minimal 3 karakter!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNama.Focus();
            return;
        }

        if (!double.TryParse(ipkStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double ipk) || ipk < 0.0 || ipk > 4.0)
        {
            MessageBox.Show("IPK harus berupa angka antara 0.00 hingga 4.00!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtIpk.Focus();
            return;
        }

        string prodi = (cmbProdi.SelectedItem is ComboBoxItem p) ? p.Content.ToString() ?? "" : "";
        int angkatan = (cmbAngkatan.SelectedItem is ComboBoxItem a && int.TryParse(a.Content.ToString(), out int angk)) ? angk : 2024;
        string status = (cmbStatus.SelectedItem is ComboBoxItem s) ? s.Content.ToString() ?? "Aktif" : "Aktif";

        // Perbarui data objek
        _selectedMahasiswa.Nama = nama;
        _selectedMahasiswa.Prodi = prodi;
        _selectedMahasiswa.Angkatan = angkatan;
        _selectedMahasiswa.Ipk = ipk;
        _selectedMahasiswa.Status = status;

        PerbaruiStatistik();
        ResetForm();

        MessageBox.Show($"Data mahasiswa {_selectedMahasiswa.Nrp} berhasil diperbarui!", "Sukses Ubah Data", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void btnHapus_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMahasiswa == null) return;

        MessageBoxResult result = MessageBox.Show(
            $"Apakah Anda yakin ingin menghapus data mahasiswa:\nNRP: {_selectedMahasiswa.Nrp}\nNama: {_selectedMahasiswa.Nama}?",
            "Konfirmasi Hapus Data",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            string nrpDihapus = _selectedMahasiswa.Nrp;
            _listMahasiswa.Remove(_selectedMahasiswa);
            PerbaruiStatistik();
            ResetForm();

            MessageBox.Show($"Data mahasiswa ({nrpDihapus}) berhasil dihapus dari sistem!", "Sukses Hapus Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void btnBatal_Click(object sender, RoutedEventArgs e)
    {
        ResetForm();
    }

    private void ResetForm()
    {
        _selectedMahasiswa = null;
        dgMahasiswa.SelectedItem = null;

        txtNrp.Text = "";
        txtNrp.IsReadOnly = false;
        txtNrp.Background = Brushes.White;

        txtNama.Text = "";
        txtIpk.Text = "3.80";
        cmbProdi.SelectedIndex = 0;
        cmbAngkatan.SelectedIndex = 3; // 2024
        cmbStatus.SelectedIndex = 0; // Aktif

        // Reset badge form mode
        badgeFormMode.Background = new SolidColorBrush(Color.FromRgb(239, 246, 255));
        badgeFormMode.BorderBrush = new SolidColorBrush(Color.FromRgb(191, 219, 254));
        txtFormMode.Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175));
        txtFormMode.Text = "Mode: Tambah Mahasiswa Baru";

        btnTambah.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnHapus.IsEnabled = false;

        txtNrp.Focus();
    }

    private void AmbilScreenshot()
    {
        try
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            if (width <= 0) width = 1120;
            if (height <= 0) height = 760;

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(this);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "screenshots"));
            Directory.CreateDirectory(dir);

            string file = Path.Combine(dir, "student_management_gui.png");
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error screenshot: {ex.Message}");
        }
    }
}
