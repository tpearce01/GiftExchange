using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace GiftExchange
{
    class Participant
    {
        public readonly string name;
        public readonly string emailAddress;
        Participant recipient;

        public Participant(string name, string emailAddress)
        {
            this.name = name;
            this.emailAddress = emailAddress;
        }

        internal Participant Recipient { get => recipient; set => recipient = value; }
    }

    class Utils
    {
        public static List<Participant> ShuffleParticipants(List<Participant> participants)
        {
            return participants.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public static void AssignRecipients(List<Participant> participants)
        {
            var previousRecipient = participants[participants.Count - 1];
            participants.ForEach(participant =>
            {
                participant.Recipient = previousRecipient;
                previousRecipient = participant;
            });
        }

        public static void SendParticipationEmail(List<Participant> participants)
        {
            // TODO: Not like this
            const string senderAddress = "tylerpearcedev@gmail.com";
            const string password = "";
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderAddress, password),
                EnableSsl = true,
            };

            participants.ForEach(participant =>
            {
                smtpClient.Send(senderAddress, participant.Recipient.emailAddress, "Family Gift Exchange Assignment", @$"Hi {participant.name},

Happy holidays! Thanks for participating in our family gift exchange. This year you'll choose a gift for ${participant.Recipient.name}.
As a reminder, each person or couple participating will purchase just one gift valued between $75 to $100 for their recipient. Recipients have been randomly assigned
");
            });

           
        }
    }

    class Program
    {
        // Static list of individuals participating in the gift exchange
        static List<Participant> participants = new List<Participant>{
            new Participant("Tyler", "tylerpearcedev@gmail.com"),
            new Participant("Tyler 2", "pearcetyler@yahoo.com")
        };

        static void Main(string[] args)
        {
            var shuffledParticipants = Utils.ShuffleParticipants(Program.participants);
            Utils.AssignRecipients(shuffledParticipants);
            Utils.SendParticipationEmail(shuffledParticipants);
        }
    }
}
