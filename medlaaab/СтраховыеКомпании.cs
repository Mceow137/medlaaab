//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace medlaaab
{
    using System;
    using System.Collections.Generic;
    
    public partial class СтраховыеКомпании
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public СтраховыеКомпании()
        {
            this.Пациенты = new HashSet<Пациенты>();
        }
    
        public int id { get; set; }
        public string название { get; set; }
        public string адрес { get; set; }
        public string ИНН { get; set; }
        public string расчетный_счет { get; set; }
        public string БИК { get; set; }
        public string телефон { get; set; }
        public string email { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Пациенты> Пациенты { get; set; }
    }
}
