using AbcYazilim.OgrenciTakip.Bll.Base;
using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Data.Contexts;
using AbcYazilim.OgrenciTakip.Model.Entities;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.Bll.General
{
    /// <summary>
    /// Şehir (İl) kayıtları için gerekli iş mantığı (business logic) operasyonlarını yürüten sınıftır.
    /// </summary>
    /// <remarks>
    /// <see cref="BaseBll{T, TContext}"/> sınıfından türetilerek jenerik CRUD işlemlerini devralır.
    /// Ayrıca <see cref="IBaseGenelBll"/> ve <see cref="IBaseCommonBll"/> arayüzlerini uygulayarak 
    /// standart iş katmanı yeteneklerini kazanır.
    /// </remarks>
    public class IlBll : BaseBll<Il, OgrenciTakipContext>, IBaseGenelBll,IBaseCommonBll
    {
        public IlBll() { }
        /// <summary>
        /// <see cref="IlBll"/> sınıfının yeni bir örneğini, bağlı olduğu kullanıcı arayüzü kontrolü ile başlatır.
        /// </summary>
        /// <param name="ctrl">
        /// İş mantığı işlemleri sırasında oluşabilecek hata mesajlarının veya form kontrollerinin 
        /// yönetileceği kullanıcı arayüzü bileşeni (Form, Panel vb.).
        /// </param>
        /// <remarks>
        /// Parametre olarak alınan kontrol, temel sınıf olan <see cref="BaseBll{T, TContext}"/> 
        /// kurucusuna gönderilerek merkezi hata yönetimi mekanizmasına dahil edilir.
        /// </remarks>
        public IlBll(Control ctrl) : base(ctrl) { }
        /// <summary>
        /// Belirtilen filtre kriterlerine uygun olan tek bir İl kaydını getirir.
        /// </summary>
        /// <param name="filter">Sorgulanacak İl kaydını bulmak için kullanılan filtre ifadesi (Örn: x => x.Id == id).</param>
        /// <returns>Bulunan İl kaydını <see cref="BaseEntity"/> tipinde döndürür. Kayıt bulunamazsa null döner.</returns>
        /// <remarks>
        /// Bu metot, temel sınıftaki <see cref="BaseBll{T, TContext}.BaseSingle{TResult}"/> metodunu kullanarak 
        /// tüm varlık verisini (selector: x => x) geri döndürür.
        /// </remarks>
        public BaseEntity Single(Expression<Func<Il, bool>> filter)
        {
            return BaseSingle(filter, x => x);
        }
        /// <summary>
        /// Belirtilen filtreye uygun tüm İl kayıtlarını kod sırasına göre sıralı bir liste olarak getirir.
        /// </summary>
        /// <param name="filter">Listelenecek verileri daraltmak için kullanılacak filtre ifadesi. Tüm iller için null geçilebilir.</param>
        /// <returns>Filtrelenmiş ve kod alanına göre artan sırada sıralanmış İl kayıtlarını içeren bir koleksiyon.</returns>
        /// <remarks>
        /// Metot, veritabanı seviyesinde sıralama (ORDER BY Kod) işlemini gerçekleştirdikten sonra 
        /// veriyi <see cref="List{T}"/> formatında belleğe yükler.
        /// </remarks>
        public IEnumerable<BaseEntity> List(Expression<Func<Il, bool>> filter)
        {
            return BaseList(filter, x =>x).OrderBy(x=>x.Kod).ToList();
        }
        /// <summary>
        /// Yeni bir İl kaydını, kod çakışması kontrolü yaparak veritabanına ekler.
        /// </summary>
        /// <param name="entity">Eklenecek olan İl varlığı.</param>
        /// <returns>Ekleme işlemi başarılı ve benzersizlik kuralına uygunsa true, aksi halde false döner.</returns>
        /// <remarks>
        /// Metot, aynı kod değerine sahip ikinci bir kaydın oluşmasını engellemek için 
        /// <see cref="BaseBll{T, TContext}.BaseInsert"/> metoduna kod bazlı bir filtre gönderir.
        /// </remarks>
        public bool Insert(BaseEntity entity)
        {
            return BaseInsert(entity,x=>x.Kod==entity.Kod);
        }
        /// <summary>
        /// Mevcut bir İl kaydını, değişen alanları tespit ederek ve kod çakışmasını kontrol ederek günceller.
        /// </summary>
        /// <param name="oldEntity">Kaydın güncellenmeden önceki hali.</param>
        /// <param name="currentEntity">Kaydın güncellenmiş yeni hali.</param>
        /// <returns>Güncelleme başarılı ise true, kod çakışması veya veritabanı hatası durumunda false döner.</returns>
        public bool Update(BaseEntity oldEntity, BaseEntity currentEntity)
        {
            return BaseUpdate(oldEntity,currentEntity,x=>x.Kod==currentEntity.Kod);
        }
        /// <summary>
        /// Belirtilen İl kaydını, kullanıcıdan onay alarak veritabanından siler.
        /// </summary>
        /// <param name="entity">Silinecek olan İl varlığı.</param>
        /// <returns>Silme işlemi kullanıcı tarafından onaylanır ve veritabanında başarıyla tamamlanırsa true döner.</returns>
        public bool Delete(BaseEntity entity)
        {
            return BaseDelete(entity, KartTuru.Il);
        }
        /// <summary>
        /// İl kartları için veritabanındaki son kayıt kodunu inceleyerek bir sonraki boş İl kodunu üretir.
        /// </summary>
        /// <returns>Yeni oluşturulan İl kodu (Örn: "IL-0002").</returns>
        public string YeniKodVer()
        {
           return BaseYeniKodVer(KartTuru.Il,x=>x.Kod);
        }
    }
}
