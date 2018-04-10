using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using SB1Util.Serializer;

namespace SB1Util.ItsControlException
{
    [Serializable]
    public class B1Exception
    {
        internal LinkedList<B1InnerException> _ListInnerException = new LinkedList<B1InnerException>();

        /// <summary>
        /// A mensagem de exceção disparada no momento da exceção
        /// </summary>
        public string ExceptionMessage { get; set; }
        /// <summary>
        /// Informações do código aonde estourou o erro
        /// </summary>
        public string StackTrace { get; set; }
        /// <summary>
        /// Classe que representa o Inner Exception da aplicação
        /// </summary>
        public List<B1InnerException> B1InnerException { get; set; }

        /// <summary>
        /// Construtor Padrão necessário para serializar o objeto.
        /// </summary>
        public B1Exception()
        { }
        /// <summary>
        /// Construtor Padrão que detalha o erro em dois níveis, o erro propriamente dito e a pilha de erros anteriores
        /// </summary>
        /// <param name="Message">A mensagem de erro do sistema</param>
        /// <param name="AppException">O Objeto contendo a Exceção do sistema</param>
        public B1Exception(Exception AppException)
        {
            try
            {
                if (AppException != null)
                {
                    this.StackTrace = AppException.StackTrace;
                    this.ExceptionMessage = AppException.Message;
                    if (AppException.InnerException != null)
                        unboxInnerException(AppException.InnerException);

                    int Counter = 0;
                    foreach (B1InnerException Item in _ListInnerException)
                    {
                        Item.ErrorCounter = Counter;
                        Counter++;
                    }
                    this.B1InnerException = _ListInnerException.ToList<B1InnerException>();
                }
            }
            catch (Exception e)
            {
            }
        }
        /// <summary>
        /// Método recursivo que desempacota os erros das classes subjacentes da classe atual e ordena por ordem de erro
        /// </summary>
        /// <param name="AppException">A excessão disparada da classe</param>
        private void unboxInnerException(Exception AppException)
        {
            try
            {
                _ListInnerException.AddFirst(new B1InnerException(AppException.Message, AppException.StackTrace));

                if (AppException.InnerException != null)
                {
                    unboxInnerException(AppException.InnerException);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
    /// <summary>
    /// Classe que armazena os erros das classes subjacentes da objeto atual
    /// </summary>
    [Serializable]
    public class B1InnerException
    {
        /// <summary>
        /// Mostra em forma sequencial o Inner Exception, sendo o menor número a origem do erro e o seu crescente pelas classes que passou
        /// </summary>
        public int ErrorCounter { get; set; }
        /// <summary>
        /// A excessão aninhada um nível abaixo do erro atual
        /// </summary>
        public string InnerExceptionMessage { get; set; }
        /// <summary>
        /// O Stack Trace do erro anterior
        /// </summary>
        public string InnerExceptionStackTrace { get; set; }
        /// <summary>
        /// Construtor Padrão necessário para serializar o objeto.
        /// </summary>
        public B1InnerException() { }
        /// <summary>
        /// Classe que armazena o erro de classes subjacentes a atual
        /// </summary>
        /// <param name="InnerExceptionMessage">A mensagem da classe originária</param>
        /// <param name="InnerExceptionStackTraceMessage">A linha em que estourou o erro na classe subjacente</param>
        public B1InnerException(string InnerExceptionMessage, string InnerExceptionStackTraceMessage)
        {
            this.InnerExceptionMessage = InnerExceptionMessage;
            this.InnerExceptionStackTrace = InnerExceptionStackTraceMessage;
        }
    }


}

