using System.Collections.Generic;

namespace flexli_erp_webapi.Services.Interfaces
{
    public interface ILinkedList<T>
    {
        ///<Summary>
        /// ToDo
        ///</Summary>
        void Next( );
        
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        T Pointer( );

        ///<Summary>
        /// ToDo
        ///</Summary>
        bool Add(T element);

        ///<Summary>
        /// ToDo
        ///</Summary>
        void Clear();
        
        ///<Summary>
        /// ToDo
        ///</Summary>
        List<T> GetList();
    }
}