using DevExpress.XtraEditors;
using System;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.Common.Message
{
    public class Messages
    {
        public static void HataMesaji(string hataMesaji)
        {
            XtraMessageBox.Show(hataMesaji, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void UyariMesaji(string hataMesaji)
        {
            XtraMessageBox.Show(hataMesaji, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static DialogResult EvetSeciliEvetHayir(string mesaj, string baslik)
        {
            return XtraMessageBox.Show(mesaj, baslik, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        }
        public static DialogResult HayirSeciliEvetHayir(string mesaj, string baslik)
        {
            return XtraMessageBox.Show(mesaj, baslik, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        }
        public static DialogResult EvetSeciliEvetHayirIptal(string mesaj, string baslik)
        {
            return XtraMessageBox.Show(mesaj, baslik, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        }
        public static DialogResult SilMesaj(string kartAdi)
        {
            return HayirSeciliEvetHayir($"Seçtiğiniz {kartAdi} Silinecektir. Onaylıyormusunuz?", "Silme Onayı");
        }
        public static void KartSecmemeMesaji()
        {
            UyariMesaji("Lütfen Bir Kart Seçiniz.");
        }

        public static DialogResult KayıtMesaj()
        {
            return EvetSeciliEvetHayir("Yapılan Değişikler kayıt Edilsin mi?", "Kayıt Onayı");
        }

        public static DialogResult KapanisMesaj()
        {
            return EvetSeciliEvetHayirIptal("Yapılan Değişiklikler Kayıt Edilsin mi?", "Çıkış Onay");
        }
        public static void MukerrerKayitHataMesaji(string alanAdi)
        {
            HataMesaji($"Girmiş Olduğunuz {alanAdi} Daha Önce Kullanılmıştır.");
        }
        public static void HataliVeriMesaji(string alanAdi)
        {
            HataMesaji($" {alanAdi} Alanına Geçerli Bir Değer Girmelisiniz.");
        }
        public static DialogResult TabloExportMesaj(string dosyaFormati)
        {
            return EvetSeciliEvetHayir($"İlgili Tablo, {dosyaFormati} Olarak Dışarı Aktarılacaktir. Onaylıyormusunuz?", "Aktarım Onayı");
        }
    }
}
