using AbcYazilim.OgrenciTakip.UI.Win.Interfaces;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static DevExpress.Utils.Svg.CommonSvgImages;

namespace AbcYazilim.OgrenciTakip.UI.Win.UserControls.Grid
{
    [ToolboxItem(true)]
    public class MyGridControl : GridControl
    {
        /// <summary>
        /// Uygulama genelinde standartlaştırılmış görsel ve işlevsel özelliklere sahip özelleştirilmiş Grid kontrolüdür.
        /// </summary>
        /// <remarks>
        /// Bu bileşen, DevExpress GridControl'den türetilmiştir ve şu özellikleri varsayılan olarak sunar:
        /// <list type="bullet">
        /// <item><description>Kurumsal renk teması (Maroon) ve kalın fontlu alt bilgi paneli (Footer).</description></item>
        /// <item><description>Otomatik olarak eklenen ve düzenlemeye kapalı 'Id' ve 'Kod' kolonları.</description></item>
        /// <item><description>Enter tuşu ile sütunlar arası geçiş ve otomatik filtreleme satırı (AutoFilterRow).</description></item>
        /// <item><description>Gereksiz sağ tık menülerinin ve gruplama panelinin devre dışı bırakılması.</description></item>
        /// </list>
        /// </remarks>
        protected override BaseView CreateDefaultView()
        {
            var view = (GridView)CreateView("MyGridView");
            view.Appearance.ViewCaption.ForeColor = Color.Maroon;
            view.Appearance.HeaderPanel.ForeColor = Color.Maroon;
            view.Appearance.HeaderPanel.TextOptions.HAlignment = HorzAlignment.Center;
            view.Appearance.FooterPanel.ForeColor = Color.Maroon;
            view.Appearance.FooterPanel.Font = new Font(new FontFamily("Tahoma"), 8.25f, FontStyle.Bold);

            view.OptionsMenu.EnableColumnMenu = false;
            view.OptionsMenu.EnableFooterMenu = false;
            view.OptionsMenu.EnableGroupPanelMenu = false;

            view.OptionsNavigation.EnterMoveNextColumn = true;

            view.OptionsPrint.AutoWidth = false;
            view.OptionsPrint.PrintFooter = false;
            view.OptionsPrint.PrintGroupFooter = false;

            view.OptionsView.ShowViewCaption = true;
            view.OptionsView.ShowAutoFilterRow = true;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ColumnAutoWidth = false;
            view.OptionsView.RowAutoHeight = true;
            view.OptionsView.HeaderFilterButtonShowMode = FilterButtonShowMode.Button;

            var idColumn = new MyGridColumn
            {
                Caption = "Id",
                FieldName = "Id"
            };
            idColumn.OptionsColumn.AllowEdit = false;
            idColumn.OptionsColumn.ShowInCustomizationForm = false;
            view.Columns.Add(idColumn);

            var kodColumn = new MyGridColumn
            {
                Caption = "Kod",
                FieldName = "Kod"
            };
            kodColumn.OptionsColumn.AllowEdit = false;
            kodColumn.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            kodColumn.AppearanceCell.Options.UseTextOptions = true;
            kodColumn.Visible = true;
            view.Columns.Add(kodColumn);

            return view;
        }
        /// <summary>
        /// Grid kontrolünün kullanabileceği görünüm tiplerini sisteme kaydeder.
        /// </summary>
        /// <param name="collection">Kullanılabilir görünüm bilgilerini (Info) barındıran koleksiyon nesnesi.</param>
        /// <remarks>
        /// Bu metodun ezilmesi, özel olarak geliştirilen <see cref="MyGridView"/> tipinin 
        /// DevExpress tasarım zamanı (Design-time) ve çalışma zamanı (Runtime) altyapısına 
        /// tanıtılmasını sağlar. <c>base</c> metodun çağrılması, standart görünüm tiplerinin 
        /// kaybını önlemek için kritiktir.
        /// </remarks>
        protected override void RegisterAvailableViewsCore(InfoCollection collection)
        {
            base.RegisterAvailableViewsCore(collection);
            collection.Add(new MyGridInfoRegistrator());
        }
        /// <summary>
        /// Özelleştirilmiş GridView bileşeninin (MyGridView) DevExpress ekosistemine tescil edilmesi ve 
        /// nesne üretim sürecinin yönetilmesi için kullanılan kayıt sınıfıdır.
        /// </summary>
        /// <remarks>
        /// <see cref="GridInfoRegistrator"/> sınıfından türetilerek; özel görünümün benzersiz ismini tanımlar 
        /// ve GridControl tarafından bu görünüm talep edildiğinde yeni bir örneğinin oluşturulmasını sağlar.
        /// </remarks>
        private class MyGridInfoRegistrator : GridInfoRegistrator
        {
            /// <summary>
            /// Görünümün sistemdeki benzersiz tanımlayıcı adını döndürür.
            /// </summary>
            public override string ViewName => "MyGridView";
            /// <summary>
            /// Verilen GridControl üzerinde çalışacak olan yeni bir <see cref="MyGridView"/> örneği oluşturur.
            /// </summary>
            /// <param name="grid">Görünümün sahibi olacak olan ana GridControl nesnesi.</param>
            /// <returns>Oluşturulan özelleştirilmiş görünüm nesnesini döndürür.</returns>
            public override BaseView CreateView(GridControl grid) => new MyGridView(grid);

        }
    }
    /// <summary>
    /// DevExpress GridView'in özelleştirilmiş versiyonudur; durum çubuğu etkileşimi ve 
    /// otomatik veri formatlama özelliklerini barındırır.
    /// </summary>
    /// <remarks>
    /// Bu sınıf şu kritik işlevleri yerine getirir:
    /// <list type="bullet">
    /// <item><description><b>Tarih Otomasyonu:</b> Tarih editörü içeren kolonlarda metni ortalar ve akıllı maskeleme (DateTimeAdvancingCaret) uygular.</description></item>
    /// <item><description><b>Durum Çubuğu Desteği:</b> <see cref="IStatusBarKisayol"/> üzerinden ekranın alt kısmında kullanıcıya rehberlik eder.</description></item>
    /// <item><description><b>Güvenli Düzenleme:</b> Yeni oluşturulan tüm kolonları varsayılan olarak düzenlemeye kapatır (AllowEdit = false).</description></item>
    /// </list>
    /// </remarks>
    public class MyGridView : GridView, IStatusBarKisayol
    {
        /// <summary>
        /// MyGridView sınıfının boş bir örneğini oluşturur. 
        /// Seri hale getirme (Serialization) ve tasarımcı desteği için gereklidir.
        /// </summary>
        public MyGridView() { }
        /// <summary>
        /// Belirtilen bir GridControl sahibiyle ilişkilendirilmiş yeni bir MyGridView örneği oluşturur.
        /// </summary>
        /// <param name="ownerGrid">Bu görünümün yer alacağı ana <see cref="DevExpress.XtraGrid.GridControl"/> nesnesi.</param>
        /// <remarks>
        /// Bu kurucu metot, GridControl tarafından çalışma zamanında dinamik olarak görünüm 
        /// oluşturulurken kullanılır ve görünümün ana kontrolle olan bağını kurar.
        /// </remarks>
        public MyGridView(GridControl ownerGrid) : base(ownerGrid) { }

        #region Properties
        public string StatusBarKisayol { get; set; }
        public string StatusBarKisayolAciklama { get; set; }
        public string StatusBarAciklama { get; set; }
        #endregion

        /// <summary>
        /// Tablo sütunlarında yapılan değişiklikleri izleyerek, veri tipine uygun görsel ve işlevsel düzenlemeleri otomatik olarak uygular.
        /// </summary>
        /// <param name="column">Değişikliğe uğrayan <see cref="GridColumn"/> nesnesi.</param>
        /// <remarks>
        /// Metot, özellikle <see cref="RepositoryItemDateEdit"/> (Tarih) editörü içeren sütunları tespit eder ve şu iki işlemi yapar:
        /// <list type="number">
        /// <item><description>Hücre içeriğini yatayda merkezler.</description></item>
        /// <item><description>Maske türünü <c>DateTimeAdvancingCaret</c> olarak ayarlayarak hızlı veri girişini aktif eder.</description></item>
        /// </list>
        /// </remarks>
        protected override void OnColumnChangedCore(GridColumn column)
        {
            base.OnColumnChangedCore(column);

            if (column.ColumnEdit == null) return;
            if (column.ColumnEdit.GetType() == typeof(RepositoryItemDateEdit))
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
                ((RepositoryItemDateEdit)column.ColumnEdit).Mask.MaskType = MaskType.DateTimeAdvancingCaret;
            }
        }
        /// <summary>
        /// Görünüm için kullanılacak olan sütun koleksiyonu nesnesini oluşturur ve döndürür.
        /// </summary>
        /// <returns>Özelleştirilmiş sütun üretim mantığına sahip olan <see cref="MyGridColumnCollection"/> nesnesi.</returns>
        /// <remarks>
        /// Bu metodun ezilmesi, tablodaki tüm sütunların standart <see cref="GridColumn"/> yerine 
        /// projenin ihtiyaçlarına göre özelleştirilmiş <see cref="MyGridColumn"/> tipinde 
        /// oluşturulmasını garanti altına alır.
        /// </remarks>
        protected override GridColumnCollection CreateColumnCollection()
        {
            return new MyGridColumnCollection(this);
        }
        /// <summary>
        /// GridView üzerindeki sütunların üretim sürecini yöneten özel koleksiyon sınıfıdır.
        /// </summary>
        /// <remarks>
        /// Bu sınıf, DevExpress'in varsayılan sütun oluşturma mekanizmasını devre dışı bırakarak 
        /// tüm sütunların <see cref="MyGridColumn"/> tipinde oluşturulmasını sağlar. 
        /// Yeni oluşturulan her sütun, veri güvenliği için varsayılan olarak düzenlemeye kapatılır.
        /// </remarks>
        private class MyGridColumnCollection : GridColumnCollection
        {
            /// <summary>
            /// Belirtilen görünüm (View) için yeni bir sütun koleksiyonu örneği oluşturur.
            /// </summary>
            /// <param name="view">Koleksiyonun bağlı olduğu <see cref="ColumnView"/>.</param>
            public MyGridColumnCollection(ColumnView view) : base(view) { }
            /// <summary>
            /// Özelleştirilmiş bir <see cref="MyGridColumn"/> nesnesi oluşturur ve temel ayarlarını yapar.
            /// </summary>
            /// <returns>Düzenleme özelliği kapatılmış yeni bir <see cref="MyGridColumn"/> nesnesi.</returns>
            protected override GridColumn CreateColumn()
            {
                var column = new MyGridColumn();
                column.OptionsColumn.AllowEdit = false;
                return column;
            }
        }
    }
    /// <summary>
    /// DevExpress GridColumn sınıfının özelleştirilmiş versiyonudur; durum çubuğu etkileşimi için gerekli özellikleri barındırır.
    /// </summary>
    /// <remarks>
    /// <see cref="IStatusBarKisayol"/> arayüzünü uygulayarak, her bir kolonun durum çubuğunda (Status Bar) 
    /// kendine has açıklama ve kısayol bilgilerini taşımasını sağlar. Bu sayede kullanıcı tablo üzerinde 
    /// hücreler arası geçiş yaparken dinamik yardım metinleri görüntülenebilir.
    /// </remarks>
    public class MyGridColumn : GridColumn, IStatusBarKisayol
    {
        #region Properties
        public string StatusBarKisayol { get; set; }
        public string StatusBarKisayolAciklama { get; set; }
        public string StatusBarAciklama { get; set; }
        #endregion
    }
}
