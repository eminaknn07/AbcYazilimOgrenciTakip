using AbcYazilim.OgrenciTakip.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AbcYazilim.Dal.Interfaces
{
    /// <summary>
    /// Tüm varlıklar (entities) için temel veritabanı işlemlerini tanımlayan jenerik repository arayüzü.
    /// </summary>
    /// <typeparam name="T">Veritabanı tablosu ile eşleşen ve sınıf (class) tipinde olan varlık.</typeparam>
    /// <remarks>
    /// Bu arayüz <see cref="IDisposable"/> arayüzünden türetilmiştir, bu sayede veritabanı bağlantıları güvenli bir şekilde serbest bırakılabilir.
    /// CRUD (Ekleme, Okuma, Güncelleme, Silme) operasyonlarının standart bir yapıda yürütülmesini sağlar.
    /// </remarks>
    public interface IRepository<T>:IDisposable where T : class
    {
        /// <summary>Tekil varlık ekleme işlemini tanımlar.</summary>
        void Insert(T entity);
        /// <summary>Koleksiyon halindeki varlıkların toplu ekleme işlemini tanımlar.</summary>
        void Insert(IEnumerable<T> entities);
        /// <summary>Varlığın tüm alanlarını güncellenmek üzere işaretler.</summary>
        void Update(T entity);
        /// <summary>Varlığın sadece belirtilen alanlarını (fields) güncellenmek üzere işaretler.</summary>
        void Update(T entity, IEnumerable<string> fields);
        /// <summary>Birden fazla varlığı toplu güncellenmek üzere işaretler.</summary>
        void Update(IEnumerable<T> entities);
        /// <summary>Varlığı silinmek üzere işaretler.</summary>
        void Delete(T entity);
        /// <summary>Varlıkları toplu olarak silinmek üzere işaretler.</summary>
        void Delete(IEnumerable<T> entities);
        /// <summary>Belirtilen filtreye göre veriyi seçer ve istenen tipe dönüştürerek tekil sonuç döndürür.</summary>
        TResult Find<TResult>(Expression<Func<T,bool>> filter,Expression<Func<T,TResult>>selector);
        /// <summary>Filtreye uygun verileri seçer ve IQueryable olarak döndürerek ertelenmiş sorgulama imkanı sağlar.</summary>
        IQueryable<TResult> Select<TResult>(Expression<Func<T,bool>> filter,Expression<Func<T, TResult>> selector);
        /// <summary>Belirtilen tablo ve alan için bir sonraki artışlı kod değerini üretir.</summary>
        
        int Count(Expression<Func<T,bool>> filter=null);
        string YeniKodVer(KartTuru kartTuru, Expression<Func<T, string>> filter, Expression<Func<T, bool>> where = null);

    }
}
