using DevExpress.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VKTypingBot.Models;

namespace VKTypingBot.ViewModel
{
    /// <summary>
    /// Main view model.
    /// </summary>
    class MainVM : ValidateViewModel
    {
        static SendRequests getRequests;
        static Random rn = new Random();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainVM()
        {
            StartSpamCommand = new AsyncCommand<bool>(StartSpam);
            HowToGetAPIKeyCommand = new DelegateCommand(HowToGetAPIKey);
        }

        /// <summary>
        /// API Token.
        /// </summary>
        [Display(Name = "Token")]
        [Required(ErrorMessage = "{0} must not be empty", AllowEmptyStrings = false)]
        [StringLength(85, MinimumLength = 85, ErrorMessage = "{0} must be length = 85")]
        public string Token { get; set; }

        /// <summary>
        /// Peer.
        /// </summary>
        public Peer Peer { get; set; } = Peer.User;

        /// <summary>
        /// Typing type.
        /// </summary>
        public TypingType TypingType { get; set; } = TypingType.Typing;

        /// <summary>
        /// Peer id.
        /// </summary>
        [Display(Name = "PeerID")]
        [Required(ErrorMessage = "{0} must not be empty", AllowEmptyStrings = false)]
        public int PeerID { get; set; }

        /// <summary>
        /// Spam status.
        /// </summary>
        public bool IsStarted { get; set; } = false;

        /// <summary>
        /// Start spam command.
        /// </summary>
        public AsyncCommand<bool> StartSpamCommand { get; }

        /// <summary>
        /// Open site for gets api key.
        /// </summary>
        public DelegateCommand HowToGetAPIKeyCommand { get; }

        /// <summary>
        /// Start spam.
        /// </summary>
        private Task StartSpam(bool notUsed)
        {
            if (!IsValid)
                return Task.CompletedTask;

            return Task.Factory.StartNew(() =>
            {
                int peer = 0;
                string token = Token;
                switch (Peer)
                {
                    case Peer.User:
                        peer = PeerID;
                        break;
                    case Peer.Chat:
                        peer = 2_000_000_000 + PeerID;
                        break;
                    case Peer.Community:
                        peer = -PeerID;
                        break;
                }
                string typingType = TypingType.ToString().ToLower();

                getRequests = new SendRequests($"https://api.vk.com/api.php?oauth=1&method=messages.setActivity&v=5.92&access_token={token}&type={typingType}&peer_id={peer}");

                IsStarted = true;
                while (!StartSpamCommand.CancellationTokenSource.IsCancellationRequested)
                {
                    getRequests.SendRequest();

                    Thread.Sleep(rn.Next(5000, 6000));
                }

                IsStarted = false;
            });
        }

        /// <summary>
        /// Open site for gets api key.
        /// </summary>
        private void HowToGetAPIKey()
        {
            Process.Start("https://oauth.vk.com/authorize?client_id=3116505&scope=messages,offline&redirect_uri=https://oauth.vk.com/blank.html&display=page&response_type=token&revoke=1");
        }
    }

    class SendRequests
    {
        private WebClient client;
        private string url;

        public SendRequests(string url)
        {
            this.url = url;
            client = new WebClient { Proxy = new WebProxy() };
        }

        public void SendRequest()
        {
            Console.WriteLine(client.DownloadString(url));
        }
    }
}
