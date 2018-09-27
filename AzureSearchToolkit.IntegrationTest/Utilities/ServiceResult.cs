using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class ServiceException
    {
        private Exception _exception;

        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ServiceException() { }

        public static ServiceException Build(Exception e)
        {
            return new ServiceException
            {
                _exception = e,
                Message = e.Message,
                StackTrace = e.StackTrace
            };
        }

        public Exception GetException()
        {
            if (_exception != null)
            {
                return _exception;
            }

            return new Exception(Message);
        }
    }

    class ServiceResult<T>
    {
        public HttpStatusCode Status { get; set; }
        public string StatusKey { get; set; }
        public string StatusMessage { get; set; }
        public string[] StatusParameters { get; set; }
        public Dictionary<string, string> StatusKeyValueParameters { get; set; }
        public ServiceException PotentialException { get; set; }
        public T Data { get; set; }

        public ServiceResult()
        {
            Status = HttpStatusCode.OK;
        }

        public ServiceResult<T> CopyStatus<U>(ServiceResult<U> otherServiceResult)
        {
            Status = otherServiceResult.Status;
            StatusMessage = otherServiceResult.StatusMessage;
            StatusKey = otherServiceResult.StatusKey;
            StatusKeyValueParameters = otherServiceResult.StatusKeyValueParameters;
            StatusParameters = otherServiceResult.StatusParameters;

            return this;
        }

        public static ServiceResult<T> Create()
        {
            return new ServiceResult<T>();
        }

        public bool IsEmpty()
        {
            return Data == null;
        }

        public bool IsError()
        {
            return PotentialException != null || Status == HttpStatusCode.InternalServerError;
        }

        public bool IsNotFound()
        {
            return Status == HttpStatusCode.NotFound;
        }

        public bool IsStatusOk()
        {
            return Status == HttpStatusCode.OK;
        }

        public ServiceResult<E> Mutate<E>(Func<T, E> mutator)
        {
            var serviceResult = new ServiceResult<E>();

            if (Data != null)
            {
                serviceResult.Data = mutator(Data);
            }

            return serviceResult.CopyStatus(this);
        }

        public ServiceResult<T> SetData(T data)
        {
            Data = data;

            return this;
        }

        public ServiceResult<T> SetException(Exception e)
        {
            PotentialException = ServiceException.Build(e);
            Status = HttpStatusCode.InternalServerError;

            return this;
        }

        public ServiceResult<T> SetMessage(string message, params string[] arguments)
        {
            if (arguments != null && arguments.Any())
            {
                StatusMessage = string.Format(message, arguments);
            }
            else
            {
                StatusMessage = message;
            }

            return this;
        }

        public ServiceResult<T> SetStatus(HttpStatusCode status)
        {
            Status = status;

            return this;
        }

        public ServiceResult<T> SetStatusKey(string key)
        {
            StatusKey = key;

            return this;
        }

        public ServiceResult<T> SetStatusKeyWithParameters(string key, params string[] parameters)
        {
            StatusKey = key;
            StatusParameters = parameters;

            return this;
        }

        public ServiceResult<T> SetStatusKeyWithParameters(string key, Dictionary<string, string> parameters)
        {
            StatusKey = key;
            StatusKeyValueParameters = parameters;

            return this;
        }

        public ServiceResult<T> SetStatusWithMessage(HttpStatusCode status, string statusMessage, params string[] statusMessageArguments)
        {
            Status = status;

            return SetMessage(statusMessage, statusMessageArguments);
        }
    }
}
