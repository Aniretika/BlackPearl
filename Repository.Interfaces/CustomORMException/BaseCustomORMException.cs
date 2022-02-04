using System;
using System.Runtime.Serialization;

namespace Repository.Interfaces.CustomORM
{
    [Serializable]
    internal class BaseCustomORMException : Exception
    {
       internal BaseCustomORMException()
       {

       }

       internal BaseCustomORMException(string message)
            : base(message)
       {

       }

       public BaseCustomORMException(string message, Exception innerException)
            : base(message, innerException)
       {

       }
    }
}
