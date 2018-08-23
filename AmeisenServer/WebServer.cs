using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace AmeisenServer
{
    public class WebServer
    {
        public bool IsRunning { get; private set; }

        private readonly HttpListener httpListener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> responseFunction;

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> responseFunction)
        {
            foreach (var s in prefixes)
                httpListener.Prefixes.Add(s);

            this.responseFunction = responseFunction;
            httpListener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method)
        {
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                IsRunning = true;
                try
                {
                    while (httpListener.IsListening)
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            HttpListenerContext listenerContext = (HttpListenerContext)c;
                            try
                            {
                                if (listenerContext == null)
                                    return;

                                string responseString = responseFunction(listenerContext.Request);
                                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                                // For javashit auto-refresh
                                listenerContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                                listenerContext.Response.ContentLength64 = buffer.Length;
                                listenerContext.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                            catch { }
                            finally
                            {
                                try
                                {
                                    if (listenerContext != null)
                                        listenerContext.Response.OutputStream.Close();
                                }
                                catch { }
                            }
                        }, httpListener.GetContext());
                }
                catch { }
            });
        }

        public void Stop()
        {
            IsRunning = false;
            httpListener.Stop();
            httpListener.Close();
        }
    }
}