using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Common.Message;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using AbcYazilim.OgrenciTakip.UI.Win.Functions;
using AbcYazilim.OgrenciTakip.UI.Win.Interfaces;
using AbcYazilim.OgrenciTakip.UI.Win.UserControls.Controls;
using AbcYazilim.OgrenciTakip.UI.Win.UserControls.Grid;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.UI.Win.Forms.BaseForms
{
    /// <summary>
    /// Uygulama genelindeki tüm veri giriş ve düzenleme (Edit) formları için temel işlevselliği sağlayan sınıftır.
    /// </summary>
    /// <remarks>
    /// <see cref="RibbonForm"/> yapısını kullanarak modern bir arayüz sunar. Bu sınıf; veri yükleme, 
    /// kaydetme, silme, yazdırma ve form kapatma gibi operasyonları standart bir disiplin altına alır.
    /// Alt sınıflar (Örn: <c>OkulEditForm</c>), bu sınıftaki sanal metotları (virtual) ezerek 
    /// kendi iş kurallarını işletirler.
    /// </remarks>
    public partial class BaseEditForm : RibbonForm
    {
        /// <summary>
        /// Edit formlarının çalışma mantığını, veri takibini ve durum yönetimini sağlayan temel alanlar.
        /// </summary>
        /// <remarks>
        /// Bu değişkenler, formun yaşam döngüsü boyunca şu kritik görevleri üstlenir:
        /// <list type="bullet">
        /// <item><description><b>Veri Takibi:</b> <c>OldEntity</c> ve <c>CurrentEntity</c> ile değişiklik kontrolü yapılır.</description></item>
        /// <item><description><b>Durum Yönetimi:</b> <c>BaseIslemTuru</c> ile formun Insert mi yoksa Update modunda mı olduğu belirlenir.</description></item>
        /// <item><description><b>İş Mantığı Bağlantısı:</b> <c>Bll</c> (Business Logic Layer) üzerinden veritabanı operasyonları yönetilir.</description></item>
        /// <item><description><b>UI Kontrolü:</b> <c>DataLayoutControl</c> dizileri ile form üzerindeki bileşenlerin toplu yönetimi (Enabled/Disabled) sağlanır.</description></item>
        /// </list>
        /// </remarks>
        private bool _formSablonKayitEdilecek;
        protected internal IslemTuru BaseIslemTuru;
        protected internal long Id;
        protected internal bool RefreshYapilacak;
        protected MyDataLayoutControl DataLayoutControl;
        protected MyDataLayoutControl[] DataLayoutControls;
        protected IBaseBll Bll;
        protected KartTuru BaseKartTuru;
        protected BaseEntity OldEntity;
        protected BaseEntity CurrentEntity;
        protected bool Isloaded;
        protected bool KayitSonrasiFOrmuKapat = true;

        public BaseEditForm()
        {
            InitializeComponent();

        }
        /// <summary>
        /// Form üzerindeki Ribbon butonları, form olayları ve veri giriş kontrolleri için merkezi olay (event) aboneliklerini başlatır.
        /// </summary>
        /// <remarks>
        /// Metot üç aşamalı bir bağlama işlemi gerçekleştirir:
        /// <list type="number">
        /// <item><description><b>Ribbon Butonları:</b> Tüm bar item'ları ortak <c>Button_ItemClick</c> olayına bağlar.</description></item>
        /// <item><description><b>Form Olayları:</b> Yüklenme, kapanma ve boyut değişimlerini takip altına alır.</description></item>
        /// <item><description><b>Kontrol Olayları:</b> Layout içindeki tüm kontrolleri gezer; odaklanma, tuş vuruşu ve değer değişimi olaylarını merkezi metotlara yönlendirir.</description></item>
        /// </list>
        /// </remarks>
        protected void EventsLoad()
        {
            //Button Events
            foreach (BarItem button in ribbonControl.Items)
                button.ItemClick += Button_ItemClick;

            //FormEvents
            LocationChanged += BaseEditForm_LocationChanged;
            SizeChanged += BaseEditForm_SizeChanged;
            Load += BaseEditForm_Load;
            FormClosing += BaseEditForm_FormClosing;

            void ControlEvents(Control control)
            {
                control.KeyDown += Control_KeyDown;
                control.GotFocus += Control_GotFocus;
                control.Leave += Control_Leave;

                switch (control)
                {
                    case MyButtonEdit edt:
                        edt.IdChanged += Control_IdChanged;
                        edt.EnabledChange += Control_EnabledChange;
                        edt.ButtonClick += Control_ButtonClick;
                        edt.DoubleClick += Control_DoubleClick;
                        break;
                    case BaseEdit edt:
                        edt.EditValueChanged += Control_EditValueChanged;
                        break;
                }

            }
            if (DataLayoutControls == null)
            {
                if (DataLayoutControl == null) return;
                foreach (Control ctrl in DataLayoutControl.Controls)
                    ControlEvents(ctrl);
            }
            else
                foreach (var layout in DataLayoutControls)
                    foreach (Control ctrl in layout.Controls)
                        ControlEvents(ctrl);
        }

        /// <summary>
        /// Veri giriş kontrolünden odak (focus) ayrıldığında, durum çubuğundaki (Status Bar) yardımcı bilgileri temizler.
        /// </summary>
        /// <param name="sender">Odak kaybolan kontrol nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Kullanıcı bir kontrolden ayrıldığında, o kontrole özgü kısayol tuşu ve açıklama metinlerinin 
        /// görünürlüğünü <c>BarItemVisibility.Never</c> yaparak gizler. Bu sayede bir sonraki kontrole 
        /// geçene kadar durum çubuğunun temiz kalması sağlanır.
        /// </remarks>
        private void Control_Leave(object sender, EventArgs e)
        {
            statusBarKisayol.Visibility = BarItemVisibility.Never;
            statusBarKisayolAciklama.Visibility = BarItemVisibility.Never;
        }

        /// <summary>
        /// Bir kontrol odak (focus) aldığında, o kontrole tanımlanmış olan açıklama ve kısayol bilgilerini durum çubuğunda görüntüler.
        /// </summary>
        /// <param name="sender">Odaklanan kontrol nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Metot, nesnenin tipini ve uyguladığı arayüzleri (<see cref="IStatusBarKisayol"/>, <see cref="IStatusBarAciklama"/>) kontrol eder:
        /// <list type="bullet">
        /// <item><description><b>Kısayol Destekli Kontroller:</b> Görünürlüğü açar ve hem kısayol hem açıklama bilgilerini yükler.</description></item>
        /// <item><description><b>Sadece Açıklama Destekli Kontroller:</b> Sadece ilgili açıklama metnini günceller.</description></item>
        /// </list>
        /// </remarks>
        private void Control_GotFocus(object sender, EventArgs e)
        {
            var type = sender.GetType();
            if (type == typeof(MyButtonEdit) || type == typeof(MyGridView) || type == typeof(MyPictureEdit) || type == typeof(MyComboBoxEdit) || type == typeof(MyDateEdit))
            {
                statusBarKisayol.Visibility = BarItemVisibility.Always;
                statusBarAciklama.Visibility = BarItemVisibility.Always;

                statusBarAciklama.Caption = ((IStatusBarAciklama)sender).StatusBarAciklama;
                statusBarKisayol.Caption = ((IStatusBarKisayol)sender).StatusBarKisayol;
                statusBarKisayolAciklama.Caption = ((IStatusBarKisayol)sender).StatusBarKisayolAciklama;
            }
            else if (sender is IStatusBarAciklama ctrl)
            {
                statusBarAciklama.Caption = ctrl.StatusBarAciklama;
            }
        }
        /// <summary>
        /// Formun boyutlarında meydana gelen değişiklikleri izler ve kullanıcı özel şablonunun kaydedilmesi gerektiğini işaretler.
        /// </summary>
        /// <param name="sender">Boyutu değişen form nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Kullanıcı arayüzünde (UI) yapılan kişiselleştirmelerin (Form boyutu, konumu vb.) kalıcı olması için 
        /// <c>_formSablonKayitEdilecek</c> değişkenini tetikler. Form kapatılırken bu değişken kontrol edilerek 
        /// güncel boyutlar kullanıcı bazlı ayarlara kaydedilir.
        /// </remarks>
        private void BaseEditForm_SizeChanged(object sender, EventArgs e)
        {
            _formSablonKayitEdilecek = true;
        }
        /// <summary>
        /// Formun ekrandaki konumu (koordinatları) değiştiğinde tetiklenir ve kullanıcı yerleşim şablonunun güncellenmesi gerektiğini işaretler.
        /// </summary>
        /// <param name="sender">Konumu değişen form nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Formun X ve Y koordinatlarında yapılan her türlü değişiklikte <c>_formSablonKayitEdilecek</c> bayrağını <c>true</c> yapar. 
        /// Bu sayede form kapatılırken güncel konum bilgileri kullanıcı tercihlerine kaydedilerek 
        /// bir sonraki oturumda formun aynı konumda açılması sağlanır.
        /// </remarks>
        private void BaseEditForm_LocationChanged(object sender, EventArgs e)
        {
            _formSablonKayitEdilecek = true;
        }
        /// <summary>
        /// Formun kapanma sürecini yönetir; kullanıcı şablonlarını kaydeder ve kaydedilmemiş veriler için onay/kayıt mekanizmasını tetikler.
        /// </summary>
        /// <param name="sender">Kapatılmak istenen form nesnesi.</param>
        /// <param name="e">Kapatma işleminin iptal edilip edilemeyeceğini kontrol eden olay parametreleri.</param>
        /// <remarks>
        /// Metot şu iş akışını takip eder:
        /// <list type="number">
        /// <item><description><b>Görsel Hafıza:</b> <c>SablonKaydet()</c> ile formun son boyut ve konum bilgilerini kalıcı hale getirir.</description></item>
        /// <item><description><b>Veri Güvenliği:</b> Eğer formda değişiklik yapılmışsa (Kaydet butonu aktifse), kullanıcıya değişiklikleri kaydetmek isteyip istemediğini sorar.</description></item>
        /// <item><description><b>İşlem İptali:</b> Kayıt işlemi başarısız olursa veya kullanıcı süreci iptal ederse <c>e.Cancel = true</c> yaparak formun kapanmasını engeller.</description></item>
        /// </list>
        /// </remarks>
        private void BaseEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SablonKaydet();
            if (btnKaydet.Visibility == BarItemVisibility.Never || btnKaydet.Enabled) return;
            if (!Kaydet(true))
                e.Cancel = true;
        }
        /// <summary>
        /// Formun görsel yerleşim bilgilerini (konum, boyut ve pencere durumu) kullanıcı bazlı ayarlara kaydeder.
        /// </summary>
        /// <remarks>
        /// Metot, sadece <c>_formSablonKayitEdilecek</c> değişkeni <c>true</c> olduğunda çalışır. 
        /// Bu sayede gereksiz veritabanı/dosya yazma işlemlerinin önüne geçilir. 
        /// Kaydedilen veriler; formun bir sonraki açılışında aynı görsel düzende gelmesini sağlar.
        /// </remarks>
        protected void SablonKaydet()
        {
            if (_formSablonKayitEdilecek)
                Name.FormSablonKaydet(Left, Top, Width, Height, WindowState);
        }
        /// <summary>
        /// Formun daha önce kaydedilmiş olan görsel yerleşim şablonunu (konum, boyut, pencere durumu) geri yükler.
        /// </summary>
        /// <remarks>
        /// Metot, formun <c>Name</c> özelliği üzerinden ilgili ayarları bulur ve formu kullanıcının bıraktığı 
        /// son görsel duruma getirir. Genellikle formun <c>Load</c> olayında veya gösterilmeden hemen önce tetiklenir.
        /// </remarks>
        private void sablonYukle()
        {
            Name.FormSablonYukle(this);
        }
        /// <summary>
        /// Form üzerindeki kontrollerin aktiflik (Enabled) durumu değiştiğinde tetiklenen sanal metottur.
        /// </summary>
        /// <param name="sender">Aktiflik durumu değişen kontrol nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Bu metot <c>virtual</c> olarak tanımlanmıştır; böylece alt formlar, belirli bir kontrol 
        /// pasif veya aktif hale geldiğinde (Örn: Bir seçim sonrası diğer alanların kilitlenmesi) 
        /// kendi özel iş mantıklarını bu metodu ezerek işletebilirler.
        /// </remarks>
        protected virtual void Control_EnabledChange(object sender, EventArgs e) { }

        /// <summary>
        /// Form üzerindeki giriş kontrollerinin değerleri değiştiğinde tetiklenir ve güncel veri modelini yeniler.
        /// </summary>
        /// <param name="sender">Değeri değişen kontrol (TextBox, ComboBox, DateEdit vb.).</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Metot, gereksiz işlem yükünü önlemek için <c>Isloaded</c> kontrolü yapar. 
        /// Form tamamen yüklendikten sonra yapılan her değişiklikte <see cref="GuncelNesneOlustur"/> 
        /// metodunu çağırarak, ekrandaki son verilerin <c>CurrentEntity</c> nesnesine aktarılmasını sağlar. 
        /// Bu süreç, "Kaydet" butonunun aktifleşmesi ve "Değişiklik Takibi (Dirty Check)" için temel teşkil eder.
        /// </remarks>
        private void Control_EditValueChanged(object sender, EventArgs e)
        {
            if (!Isloaded) return;
            GuncelNesneOlustur();
        }
        /// <summary>
        /// Kontrol üzerine çift tıklandığında ilgili rehber veya seçim ekranını tetikler.
        /// </summary>
        /// <param name="sender">Çift tıklanan kontrol nesnesi (Örn: MyButtonEdit).</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Kullanıcının seçim butonuna basma ihtiyacını ortadan kaldırarak, doğrudan 
        /// <see cref="SecimYap"/> metodunu çalıştırır. Bu sayede veri giriş hızı artırılır 
        /// ve daha akıcı bir kullanıcı deneyimi sunulur.
        /// </remarks>
        private void Control_DoubleClick(object sender, EventArgs e)
        {
            SecimYap(sender);
        }
        /// <summary>
        /// ButtonEdit türündeki kontrollerin üzerindeki butonlara tıklandığında ilgili seçim ekranını açar.
        /// </summary>
        /// <param name="sender">Butonuna tıklanan kontrol nesnesi.</param>
        /// <param name="e">Hangi butonun tıklandığı bilgisini içeren olay parametreleri.</param>
        /// <remarks>
        /// Kullanıcı rehber butona tıkladığında merkezi <see cref="SecimYap"/> metodunu tetikleyerek 
        /// ilgili kart türüne ait liste formunun modal olarak açılmasını sağlar.
        /// </remarks>
        private void Control_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            SecimYap(sender);
        }
        /// <summary>
        /// ButtonEdit türündeki kontrollerin Id değeri (bağlı olduğu kayıt) değiştiğinde tetiklenir.
        /// </summary>
        /// <param name="sender">Id değeri değişen rehber kontrolü.</param>
        /// <param name="e">Eski ve yeni Id değerlerini barındıran olay parametreleri.</param>
        /// <remarks>
        /// Metot, bir seçim işlemi sonucunda veya kod tarafında Id atandığında çalışır. 
        /// <c>Isloaded</c> kontrolü sayesinde formun ilk açılışındaki yükleme yükünü eler. 
        /// Seçim sonrası güncel Id bilgisinin anlık olarak <c>CurrentEntity</c> nesnesine 
        /// yansıtılması için <see cref="GuncelNesneOlustur"/> metodunu tetikler.
        /// </remarks>
        private void Control_IdChanged(object sender, IdChangedEventArgs e)
        {
            if (!Isloaded) return;
            GuncelNesneOlustur();
        }
        /// <summary>
        /// Form üzerindeki kontrollerde basılan tuşları yakalayarak özel kısayol komutlarını çalıştırır.
        /// </summary>
        /// <param name="sender">Tuş vuruşunun gerçekleştiği kontrol nesnesi.</param>
        /// <param name="e">Basılan tuş ve modifikatör (Ctrl, Shift, Alt) bilgilerini barındıran olay parametreleri.</param>
        /// <remarks>
        /// Metot şu standart kısayolları yönetir:
        /// <list type="bullet">
        /// <item><description><b>ESC:</b> Formu kapatır.</description></item>
        /// <item><description><b>Ctrl + Shift + Delete:</b> <see cref="MyButtonEdit"/> kontrolündeki seçili kaydı (Id ve Text) temizler.</description></item>
        /// <item><description><b>F4 veya Alt + Aşağı Ok:</b> Rehber seçim ekranını (<see cref="SecimYap"/>) tetikler.</description></item>
        /// </list>
        /// </remarks>
        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            if (sender is MyButtonEdit edt)
                switch (e.KeyCode)
                {
                    case Keys.Delete when e.Control && e.Shift:
                        edt.Id = null;
                        edt.EditValue = null;
                        break;
                    case Keys.F4:
                    case Keys.Down when e.Modifiers == Keys.Alt:
                        SecimYap(edt);
                        break;
                }
        }
        /// <summary>
        /// Formun yüklenme anında gerekli ilk ayarları yapar, veri modelini hazırlar ve görsel şablonu uygular.
        /// </summary>
        /// <param name="sender">Yüklenen form nesnesi.</param>
        /// <param name="e">Olay parametreleri.</param>
        /// <remarks>
        /// Metot şu iş akışını takip eder:
        /// <list type="number">
        /// <item><description><b>Takip Başlatma:</b> <c>Isloaded</c> bayrağını aktif ederek veri değişim izlemesini başlatır.</description></item>
        /// <item><description><b>İlk Durum Kaydı:</b> Ekrandaki mevcut verilerle ilk <c>CurrentEntity</c> nesnesini oluşturur.</description></item>
        /// <item><description><b>Kişiselleştirme:</b> <c>sablonYukle</c> ile formun boyut ve konum ayarlarını geri yükler.</description></item>
        /// <item><description><b>Kimlik Yönetimi:</b> İşlem türüne göre (Yeni/Güncelleme) formun <c>Id</c> bilgisini netleştirir.</description></item>
        /// </list>
        /// </remarks>
        private void BaseEditForm_Load(object sender, EventArgs e)
        {
            Isloaded = true;
            GuncelNesneOlustur();

            sablonYukle();
            //ButonGizleGoster();
            //Güncelleme yapılacak
        }
        /// <summary>
        /// Formun üst menüsündeki (Ribbon Control) butonlara tıklandığında ilgili operasyonel süreçleri tetikler.
        /// </summary>
        /// <param name="sender">Tıklanan butonun bağlı olduğu Ribbon nesnesi.</param>
        /// <param name="e">Tıklanan butonun (BarItem) bilgilerini barındıran olay parametreleri.</param>
        /// <remarks>
        /// Metot, kullanıcı etkileşimine göre şu ana işlevleri yerine getirir:
        /// <list type="bullet">
        /// <item><description><b>Yeni:</b> Formu yeni bir kayıt oluşturma moduna (<c>EntityInsert</c>) sokar ve verileri temizler.</description></item>
        /// <item><description><b>Kaydet:</b> Mevcut verileri veritabanına işler.</description></item>
        /// <item><description><b>Geri Al:</b> Yapılan değişiklikleri iptal ederek veritabanındaki orijinal haline döndürür.</description></item>
        /// <item><description><b>Sil:</b> Mevcut kaydı onay alarak veritabanından kaldırır.</description></item>
        /// <item><description><b>Çıkış:</b> Formu kapatır (Kapatma öncesi değişiklik kontrolü <c>FormClosing</c> içinde yapılır).</description></item>
        /// </list>
        /// </remarks>
        private void Button_ItemClick(object sender, ItemClickEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (e.Item == btnYeni)
            {
                //Yetki Kontrolü
                BaseIslemTuru = IslemTuru.EntityInsert;
                Yukle();
            }
            else if (e.Item == btnKaydet)
                Kaydet(true);
            else if (e.Item == btnGeriAl)
                GeriAl();
            else if (e.Item == btnSil)
            {
                //Yetki Kontrolü
                EntityDelete();
            }
            else if (e.Item == btnCikis)
                Close();
            Cursor.Current = Cursors.Default;
        }
        /// <summary>
        /// Form üzerindeki rehber (lookup) kontrolleri için seçim sürecini başlatan sanal metottur.
        /// </summary>
        /// <param name="sender">Seçim işleminin tetiklendiği kontrol nesnesi (Genellikle <see cref="MyButtonEdit"/>).</param>
        /// <remarks>
        /// Bu metot <c>virtual</c> olarak tanımlanmıştır; çünkü her edit formunun seçim yapacağı listeler 
        /// ve bu listelerden dönecek verileri işleme mantığı farklıdır. Alt sınıflar (Örn: <c>OgrenciEditForm</c>), 
        /// bu metodu ezerek (override) hangi kontrolün hangi liste formunu açacağını belirler.
        /// </remarks>
        protected virtual void SecimYap(object sender) { }
        /// <summary>
        /// Mevcut kaydı iş mantığı katmanı (BLL) üzerinden veritabanından siler ve formu kapatır.
        /// </summary>
        /// <remarks>
        /// Metot, <see cref="IBaseCommonBll"/> üzerinden silme işlemini gerçekleştirir. 
        /// İşlem başarılı olduktan sonra, bağlı olduğu liste formunun güncellenmesi için 
        /// <c>RefreshYapilacak</c> bayrağını işaretler ve aktif edit formunu kapatır.
        /// </remarks>
        private void EntityDelete()
        {
            if (((IBaseCommonBll)Bll).Delete(OldEntity)) return;
            RefreshYapilacak = true;
            Close();
        }
        /// <summary>
        /// Form üzerinde yapılan tüm değişiklikleri iptal ederek, verileri orijinal haline döndürür veya formu kapatır.
        /// </summary>
        /// <remarks>
        /// Metot şu iş akışını takip eder:
        /// <list type="number">
        /// <item><description><b>Onay:</b> Kullanıcıdan "Geri Al" işlemi için onay ister.</description></item>
        /// <item><description><b>Güncelleme Modu:</b> İşlem türü <c>EntityUpdate</c> ise, <see cref="Yukle"/> metodunu çağırarak verileri veritabanındaki haliyle ekrana yeniden basar.</description></item>
        /// <item><description><b>Yeni Kayıt Modu:</b> İşlem türü <c>EntityInsert</c> ise, henüz kaydedilmiş bir veri olmadığı için doğrudan formu kapatır.</description></item>
        /// </list>
        /// </remarks>
        private void GeriAl()
        {
            if (Messages.HayirSeciliEvetHayir("Yapılan Değişiklikler Geri Alınacaktır. Onaylıyormusunuz?", "Geri Al Onayı") != DialogResult.Yes) return;
            if (BaseIslemTuru == IslemTuru.EntityUpdate)
                Yukle();
            else
                Close();
        }
        /// <summary>
        /// Form üzerindeki verilerin veritabanına kayıt sürecini, kullanıcı onay mekanizmasını ve kayıt sonrası durum güncellemelerini yönetir.
        /// </summary>
        /// <param name="kapanis">İşlemin bir form kapatma eylemi sırasında tetiklenip tetiklenmediği bilgisi.</param>
        /// <returns>Kayıt işlemi başarılı ise veya kullanıcı kaydetmeden çıkmayı onayladıysa <c>true</c>, işlem iptal edildiyse <c>false</c> döner.</returns>
        /// <remarks>
        /// Metot şu kritik işlevleri yerine getirir:
        /// <list type="number">
        /// <item><description><b>Karar Mekanizması:</b> Kullanıcıya işlem türüne göre (Kayıt/Kapanış) uygun onay mesajını gösterir.</description></item>
        /// <item><description><b>İşlem Ayrımı:</b> <c>BaseIslemTuru</c>'ne göre <c>EntityInsert</c> veya <c>EntityUpdate</c> süreçlerini başlatır.</description></item>
        /// <item><description><b>Durum Senkronizasyonu:</b> Başarılı kayıt sonrası <c>OldEntity</c> nesnesini günceller ve "Dirty Check" mekanizmasını sıfırlar.</description></item>
        /// <item><description><b>Form Yönetimi:</b> Ayarlara göre formu kapatır veya yeni veri girişine uygun hale getirir.</description></item>
        /// </list>
        /// </remarks>
        private bool Kaydet(bool kapanis)
        {
            bool KayitIslemi()
            {
                Cursor.Current = Cursors.WaitCursor;
                switch (BaseIslemTuru)
                {
                    case IslemTuru.EntityInsert:
                        if (EntityInsert())
                            return KayitSonrasiIslemler();
                        break;

                    case IslemTuru.EntityUpdate:
                        if (EntityUpdate())
                            return KayitSonrasiIslemler();
                        break;
                }
                bool KayitSonrasiIslemler()
                {
                    OldEntity = CurrentEntity;
                    RefreshYapilacak = true;
                    ButonEnabledDurumu();
                    if (KayitSonrasiFOrmuKapat)
                        Close();
                    else
                        BaseIslemTuru = BaseIslemTuru == IslemTuru.EntityInsert ? IslemTuru.EntityUpdate : BaseIslemTuru;

                    return true;
                }
                return false;
            }
            var result = kapanis ? Messages.KapanisMesaj() : Messages.KayıtMesaj();

            switch (result)
            {
                case System.Windows.Forms.DialogResult.Cancel:
                    return false;

                case System.Windows.Forms.DialogResult.Yes:
                    return KayitIslemi();

                case System.Windows.Forms.DialogResult.No:
                    if (kapanis)
                        btnKaydet.Enabled = false;
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Form üzerindeki güncel veriyi (CurrentEntity) iş mantığı katmanı (BLL) üzerinden veritabanına yeni bir kayıt olarak ekler.
        /// </summary>
        /// <returns>Kayıt işlemi veritabanı seviyesinde başarıyla tamamlandıysa <c>true</c>, aksi halde <c>false</c> döner.</returns>
        /// <remarks>
        /// Metot <c>virtual</c> olarak tanımlanmıştır. Bu sayede, kayıt işlemi öncesinde veya sonrasında 
        /// özel doğrulama (validation) veya ek işlemler gerektiren alt formlarda (Örn: Stok hareketi oluşturma, 
        /// otomatik numara verme vb.) ezilerek (override) genişletilebilir.
        /// </remarks>
        protected virtual bool EntityInsert()
        {
            return ((IBaseGenelBll)Bll).Insert(CurrentEntity);
        }
        /// <summary>
        /// Mevcut kaydın veritabanındaki orijinal hali (OldEntity) ile formdaki güncel halini (CurrentEntity) kıyaslayarak güncelleme işlemini gerçekleştirir.
        /// </summary>
        /// <returns>Güncelleme işlemi veritabanı seviyesinde başarıyla tamamlandıysa <c>true</c>, aksi halde <c>false</c> döner.</returns>
        /// <remarks>
        /// Metodun iki nesneyi parametre olarak alması şu avantajları sağlar:
        /// <list type="bullet">
        /// <item><description><b>Performans:</b> Sadece değişen kolonların SQL cümlesine eklenmesini sağlar.</description></item>
        /// <item><description><b>Veri Güvenliği:</b> Veritabanındaki versiyon farklarını (Concurrency) denetleyebilir.</description></item>
        /// <item><description><b>Loglama:</b> Hangi alanın eski değerinin ne olduğunu ve neye dönüştüğünü takip etmeyi kolaylaştırır.</description></item>
        /// </list>
        /// </remarks>
        protected virtual bool EntityUpdate()
        {
            return ((IBaseGenelBll)Bll).Update(OldEntity, CurrentEntity);
        }
        /// <summary>
        /// Formun ihtiyaç duyduğu verilerin veritabanından çekilmesi ve arayüz kontrollerine aktarılması sürecini başlatan temel metottur.
        /// </summary>
        /// <remarks>
        /// Metot, "Yeni Kayıt" modunda formun varsayılan değerlerle hazırlanmasını, "Güncelleme" modunda ise 
        /// mevcut kaydın <see cref="Bll"/> üzerinden çekilerek <see cref="NesneyiKontrollereBagla"/> 
        /// metoduna iletilmesini koordine eder. Alt formlar bu metodu override ederek kendi veri yükleme 
        /// süreçlerini (Örn: Ek tabloları yükleme, özel filtreler uygulama) yönetirler.
        /// </remarks>
        protected internal virtual void Yukle() { }
        /// <summary>
        /// Veritabanından yüklenen nesne modelindeki (Entity) verileri, form üzerindeki ilgili görsel kontrollere aktarır.
        /// </summary>
        /// <remarks>
        /// Bu metot, <see cref="Yukle"/> süreci içerisinde çalıştırılır. Alt sınıflar (Örn: <c>OkulEditForm</c>), 
        /// bu metodu override ederek <c>OldEntity</c> içerisindeki property değerlerini (Örn: OkulAdi, Kod, Aciklama) 
        /// formdaki bileşenlerin (Örn: <c>txtOkulAdi.Text</c>) <c>EditValue</c> veya <c>Text</c> özelliklerine atarlar.
        /// </remarks>
        protected virtual void NesneyiKontrollereBagla() { }
        /// <summary>
        /// Form üzerindeki görsel kontrollerde bulunan güncel verileri toplayarak <see cref="CurrentEntity"/> nesnesini oluşturur veya günceller.
        /// </summary>
        /// <remarks>
        /// Bu metot, formdaki her veri değişiminde tetiklenir. Alt sınıflar (Örn: <c>OkulEditForm</c>), 
        /// bu metodu override ederek ekrandaki son değerleri (Örn: <c>txtOkulAdi.Text</c>) 
        /// <c>CurrentEntity</c> nesnesinin ilgili özelliklerine aktarırlar. Oluşturulan bu "güncel paket", 
        /// daha sonra <c>OldEntity</c> ile kıyaslanarak değişiklik olup olmadığını belirlemek için kullanılır.
        /// </remarks>
        protected virtual void GuncelNesneOlustur() { }
        /// <summary>
        /// Form üzerindeki aksiyon butonlarının (Yeni, Kaydet, Geri Al, Sil) aktiflik durumlarını, verideki değişikliklere göre dinamik olarak yönetir.
        /// </summary>
        /// <remarks>
        /// Metot, "Dirty Check" (Değişiklik Takibi) mekanizmasını işletir:
        /// <list type="bullet">
        /// <item><description><b>Isloaded Kontrolü:</b> Form yüklenme aşamasındayken gereksiz kıyaslama yapmaz.</description></item>
        /// <item><description><b>Veri Kıyaslama:</b> <c>OldEntity</c> ve <c>CurrentEntity</c> nesnelerini property bazında karşılaştırır.</description></item>
        /// <item><description><b>UI Güncelleme:</b> Eğer nesneler arasında fark varsa (veri değişmişse) "Kaydet" ve "Geri Al" butonlarını aktif hale getirir; fark yoksa pasifize eder.</description></item>
        /// </list>
        /// </remarks>
        protected internal virtual void ButonEnabledDurumu()
        {
            if (!Isloaded) return;
            GeneralFunctions.ButtonEnabledDurumu(btnYeni, btnKaydet, btnGeriAl, btnSil, OldEntity, CurrentEntity);
        }

    }
}