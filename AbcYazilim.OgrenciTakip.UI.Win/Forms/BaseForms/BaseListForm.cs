using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using AbcYazilim.OgrenciTakip.UI.Win.Functions;
using AbcYazilim.OgrenciTakip.UI.Win.Show.Interfaces;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Windows.Forms;
using DevExpress.XtraPrinting.Native;
using DevExpress.Utils.Extensions;

namespace AbcYazilim.OgrenciTakip.UI.Win.Forms.BaseForms
{
    public partial class BaseListForm : RibbonForm
    {
        private bool _formSablonKayitEdilecek;
        private bool _tabloSablonKayitEdilecek;
        protected IBaseFormShow FormShow;
        protected KartTuru BaseKartTuru;
        protected internal GridView Tablo;
        protected bool AktifkartlariGoster = true;
        protected internal bool AktifPasifButonGoster = false;
        protected internal bool Multiselect;
        protected internal BaseEntity SelectedEntity;
        protected IBaseBll Bll;
        protected ControlNavigator Navigator;
        protected internal long? SeciliGelecekId;
        protected BarItem[] ShowItems;
        protected BarItem[] HideItems;
        public BaseListForm()
        {
            InitializeComponent();
        }
        private void eventLoad()
        {
            //Button Events
            foreach (BarItem button in ribbonControl.Items)
            {
                button.ItemClick += Button_ItemClick;
            }
            //Table Events
            Tablo.DoubleClick += Tablo_DoubleClick;
            Tablo.KeyDown += Tablo_KeyDown;
            Tablo.MouseUp += Tablo_MouseUp;
            Tablo.ColumnWidthChanged += Tablo_ColumnWidthChanged;
            Tablo.ColumnPositionChanged += Tablo_ColumnPositionChanged;
            Tablo.EndSorting += Tablo_EndSorting;
            //Form Events
            Shown += BaseListForm_Shown;
            Load += BaseListForm_Load;
            FormClosing += BaseListForm_FormClosing;
            SizeChanged += BaseListForm_SizeChanged;
            LocationChanged += BaseListForm_LocationChanged;

        }

        private void BaseListForm_LocationChanged(object sender, EventArgs e)
        {
            if(!IsMdiChild) 
            _formSablonKayitEdilecek = true;
        }

        private void BaseListForm_SizeChanged(object sender, EventArgs e)
        {
            if(!IsMdiChild)
            _formSablonKayitEdilecek = true;
        }

        private void Tablo_EndSorting(object sender, EventArgs e)
        {
            _tabloSablonKayitEdilecek = true;
        }

        private void Tablo_ColumnPositionChanged(object sender, EventArgs e)
        {
            _tabloSablonKayitEdilecek = true;
        }

        private void Tablo_ColumnWidthChanged(object sender, DevExpress.XtraGrid.Views.Base.ColumnEventArgs e)
        {
            _tabloSablonKayitEdilecek = true;
        }

        private void BaseListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SablonKaydet();
        }

        private void BaseListForm_Load(object sender, EventArgs e)
        {
            SablonYukle();
        }

        private void Tablo_MouseUp(object sender, MouseEventArgs e)
        {
            e.SagMenuGoster(sagMenu);
        }

        private void BaseListForm_Shown(object sender, EventArgs e)
        {
            Tablo.Focus();
            ButonGizleGoster();
            //SutunGizleGoster();
            if (IsMdiChild || SeciliGelecekId == null) return;
            Tablo.RowFocus("Id", SeciliGelecekId);
        }

        private void ButonGizleGoster()
        {
            //btnSec.Visibility = AktifPasifButonGoster ? BarItemVisibility.Never : IsMdiChild ? BarItemVisibility.Never : BarItemVisibility.Always;
            if (AktifPasifButonGoster)
            {
                btnSec.Visibility = BarItemVisibility.Never;
            }
            else
            {
                if (IsMdiChild)
                {
                    btnSec.Visibility = BarItemVisibility.Never;
                }
                else
                {
                    btnSec.Visibility = BarItemVisibility.Always;
                }
            }
            barEnter.Visibility = IsMdiChild ? BarItemVisibility.Never : BarItemVisibility.Always;
            barEnterAciklama.Visibility = IsMdiChild ? BarItemVisibility.Never : BarItemVisibility.Always;
            //btnAktfiPasifKartlar.Visibility = AktifPasifButonGoster ? BarItemVisibility.Always : !IsMdiChild ? BarItemVisibility.Never : BarItemVisibility.Always;
            if (AktifPasifButonGoster || IsMdiChild)
            {
                btnAktfiPasifKartlar.Visibility = BarItemVisibility.Always;
            }
            else
            {
                btnAktfiPasifKartlar.Visibility = BarItemVisibility.Never;
            }
            ShowItems?.ForEach(x => x.Visibility = BarItemVisibility.Always);
            ShowItems?.ForEach(x => x.Visibility = BarItemVisibility.Always);


        }

        private void SutunGizleGoster()
        {
            throw new NotImplementedException();
        }
        private void SablonKaydet()
        {
            if (_formSablonKayitEdilecek)
                Name.FormSablonKaydet(Left, Top, Width, Height, WindowState);

            if (_tabloSablonKayitEdilecek)
                Tablo.TabloSablonKaydet(IsMdiChild ? Name + " Tablosu" : Name + " TablosuMDI");
        }
        private void SablonYukle()
        {
            if (IsMdiChild)
                Tablo.TabloSablonYukle(Name + " Tablosu");
            else
            {
                Name.FormSablonYukle(this);
                Tablo.TabloSablonYukle(Name + " TablosuMDI");
            }
        }
        protected internal void Yukle()
        {
            DegiskenkeriDoldur();
            eventLoad();
            Tablo.OptionsSelection.MultiSelect = Multiselect;
            Navigator.NavigatableControl = Tablo.GridControl;
            Cursor.Current = Cursors.WaitCursor;
            Listele();
            Cursor.Current = DefaultCursor;

            //Güncellenecek
        }

        protected virtual void DegiskenkeriDoldur() { }


        protected virtual void ShowEditForm(long id)
        {
            var result = FormShow.ShowDialogEditForm(BaseKartTuru, id);
            ShowEditFormDefault(result);
        }

        protected void ShowEditFormDefault(long id)
        {
            if (id <= 0) return;
            AktifkartlariGoster = true;
            FormCaptionAyarlar();
            Tablo.RowFocus("Id", id);
        }
        protected virtual void EntityDelete()
        {
            var entity = Tablo.GetRow<BaseEntity>();
            if (entity == null) return;
            if (!((IBaseCommonBll)Bll).Delete(entity)) return;
            Tablo.DeleteSelectedRows();
            Tablo.RowFocus(Tablo.FocusedRowHandle);
        }
        private void SelectEntity()
        {
            if (Multiselect)
            {
                // Güncellenecek
            }
            else
                SelectedEntity = Tablo.GetRow<BaseEntity>();
            DialogResult = DialogResult.OK;
        }
        protected virtual void Listele() { }
        private void FiltreSec()
        {
            throw new NotImplementedException();
        }
        private void Yazdir()
        {
            throw new NotImplementedException();
        }
        private void FormCaptionAyarlar()
        {
            if (btnAktfiPasifKartlar == null)
            {
                Listele();
                return;
            }
            if (AktifkartlariGoster)
            {
                btnAktfiPasifKartlar.Caption = "Pasif Kartlar";
                Tablo.ViewCaption = Text;
            }
            else
            {
                btnAktfiPasifKartlar.Caption = "Aktif Kartlar";
                Tablo.ViewCaption = Text + " - Pasif Kartlar";
            }
            Listele();
        }
        private void IslemTuruSec()
        {
            if (!IsMdiChild)
            {
                // Güncellenecek
                SelectEntity();
            }
            else
            {
                btnDuzelt.PerformClick();
            }
        }
        /// <summary>
        /// Liste formu üzerindeki tüm menü ve buton aksiyonlarını merkezi olarak yönetir.
        /// </summary>
        /// <param name="sender">Olayın tetiklendiği kontrol nesnesi.</param>
        /// <param name="e">Tıklanan buton veya menü öğesine (BarItem) ait olay verileri.</param>
        /// <remarks>
        /// Metot şu ana operasyonel süreçleri koordine eder:
        /// <list type="bullet">
        /// <item><description><b>Dışarı Aktarma:</b> Tablo verilerini Excel, Word, PDF ve TXT formatlarına dönüştürür.</description></item>
        /// <item><description><b>CRUD Operasyonları:</b> Yeni kayıt açma (ShowEditForm -1), düzenleme (ShowEditForm ID) ve silme işlemlerini başlatır.</description></item>
        /// <item><description><b>Tablo Yönetimi:</b> Yenileme, filtreleme, kolon özelleştirme (ShowCustomization) ve aktif/pasif kart geçişlerini yönetir.</description></item>
        /// <item><description><b>Raporlama:</b> Seçili kartın yazdırılması veya bağlı kartların açılması süreçlerini tetikler.</description></item>
        /// </list>
        /// </remarks>
        private void Button_ItemClick(object sender, ItemClickEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (e.Item == btnGonder)
            {
                var link = (BarSubItemLink)e.Item.Links[0];
                link.Focus();
                link.OpenMenu();
                link.Item.ItemLinks[0].Focus();
            }
            else if (e.Item == btnStandartExcelDosyasi) Tablo.TabloDisariAktar(DosyaTuru.ExcelStandart,e.Item.Caption,Text);
            else if (e.Item == btnFormatliExcelDosyasi) Tablo.TabloDisariAktar(DosyaTuru.ExcelFormatli, e.Item.Caption, Text);
            else if (e.Item == btnFormatsizExcelDosyasi) Tablo.TabloDisariAktar(DosyaTuru.ExcelFormatsiz, e.Item.Caption, Text);
            else if (e.Item == btnWordDosyasi) Tablo.TabloDisariAktar(DosyaTuru.WordDosyasi, e.Item.Caption, Text);
            else if (e.Item == btnPDFDosyasi) Tablo.TabloDisariAktar(DosyaTuru.PdfDosyasi, e.Item.Caption, Text);
            else if (e.Item == btnTxtDosyasi) Tablo.TabloDisariAktar(DosyaTuru.TxtDosyasi, e.Item.Caption, Text);
            else if (e.Item == btnYeni)
            {
                //Yetki Kontrolü
                ShowEditForm(-1);
            }
            else if (e.Item == btnDuzelt)
                ShowEditForm(Tablo.GetRowId());
            else if (e.Item == btnSil)
            {
                //Yetki kontrolü
                EntityDelete();
            }
            else if (e.Item == btnSec)
            {
                SelectEntity();
            }
            else if (e.Item == btnYenile)
                Listele();
            else if (e.Item == btnFiltrele)
                FiltreSec();
            else if (e.Item == btnKolonlar)
            {
                if (Tablo.CustomizationForm == null)
                    Tablo.ShowCustomization();
                else
                    Tablo.HideCustomization();
            }
            else if (e.Item == btnBagliKartlar)
                BagliKartAc();
            else if (e.Item == btnYazdir)
                Yazdir();
            else if (e.Item == btnCikis)
                Close();
            else if (e.Item == btnAktfiPasifKartlar)
            {
                AktifkartlariGoster = !AktifkartlariGoster;
                FormCaptionAyarlar();
            }
            Cursor.Current = DefaultCursor;
        }

        protected virtual void BagliKartAc() { }

        private void Tablo_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            IslemTuruSec();
            Cursor.Current = Cursors.Default;
        }
        private void Tablo_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    IslemTuruSec();
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }


    }
}