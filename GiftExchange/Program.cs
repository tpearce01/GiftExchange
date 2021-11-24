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
        public readonly List<string> emailAddress;
        public readonly List<string> invalidRecipients;
        Participant recipient;

        public Participant(string name, List<string> emailAddress, List<string> invalidRecipients)
        {
            this.name = name;
            this.emailAddress = emailAddress;
            this.invalidRecipients = invalidRecipients;
        }

        internal Participant Recipient { get => recipient; set => recipient = value; }
    }

    class Utils
    {
        /// <summary>
        /// Randomly shuffle the list of participants
        /// </summary>
        /// <param name="participants"></param>
        /// <returns></returns>
        public static List<Participant> ShuffleParticipants(List<Participant> participants)
        {
            return participants.OrderBy(a => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// Shuffles participants then assigns recipients in a circular format
        /// </summary>
        /// <param name="participants">Unshuffled list of participants</param>
        /// <param name="shuffledParticipants">Shuffled list of participants to be output</param>
        /// <returns>bool isValidAssignment - indicates if the assignment was valid</returns>
        public static bool AssignRecipients(List<Participant> participants, out List<Participant> shuffledParticipants)
        {
            bool isValidAssignment = true;
            shuffledParticipants = Utils.ShuffleParticipants(participants);

            Participant previousRecipient = shuffledParticipants[shuffledParticipants.Count - 1];
            shuffledParticipants.ForEach(participant =>
            {
                participant.Recipient = previousRecipient;
                
                // Lazy check -- instead of avoiding mismatches, let's allow them and try again if a bad match is made
                if(participant.invalidRecipients.Any(recipient => recipient.Equals(previousRecipient.name))) {
                    isValidAssignment = false;
                }

                previousRecipient = participant;
            });

            return isValidAssignment;
        }

        /// <summary>
        /// Sends an email to all participants with details of the exchange
        /// </summary>
        /// <param name="participants"></param>
        public static void SendParticipationEmail(List<Participant> participants)
        {
            // TODO: Not like this
            const string senderAddress = "tylerpearcedev@gmail.com";
            const string password = "";
            const string subject = "Family Gift Exchange Assignment";
            var fromAddress = new MailAddress(senderAddress, "Tyler Pearce");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, password)
            };

            participants.ForEach(participant =>
            {
                // Send an email to each address associated with a participant - This is useful for couples joining as a single participant
                participant.emailAddress.ForEach(email =>
                {
                    var toAddress = new MailAddress(email, participant.name);
                    string body = @$"Hi {participant.name},

Happy holidays! Thanks for participating in our family gift exchange. This year you'll choose a gift for {participant.Recipient.name}.
As a reminder, each person or couple participating will purchase just one gift valued between $75 to $100 for their recipient. Recipients have been randomly assigned using this application: https://github.com/tpearce01/GiftExchange. 

Let me know if you have any questions!

Cheers,
Tyler
";
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }
                });
            });           
        }
    }

    class Program
    {
        // Static list of individuals participating in the gift exchange
        static List<Participant> participants = new List<Participant>{
            // Pearce
            new Participant("Courtney", new List<string> { "courtneyp0624@yahoo.com" }, new List<string> { "Margo & JR" }),
            new Participant("Margo & JR", new List<string> { "margomee@yahoo.com", "jrpearce@cox.net" }, new List<string> { "Courtney" }),
            // Barnas
            new Participant("Kyle & Peggy", new List<string> { "peggyphan1@gmail.com", "kylebengt@gmail.com" }, new List<string> { "Nancy", "Danielle" }),
            new Participant("Nancy", new List<string> { "oceast2west@gmail.com" }, new List<string> { "Kyle & Peggy", "Danielle" }),
            new Participant("Danielle", new List<string> { "dmbarnas@gmail.com" }, new List<string> { "Kyle & Peggy", "Nancy" }),
            // Colombo
            new Participant("Carmen & Mike", new List<string> { "carmen.n.colombo@gmail.com" }, new List<string> { "Ori & Paul", "Zach" }),
            new Participant("Ori & Paul", new List<string> { "oricolombo@verizon.net", "pcolombo@verizon.net" }, new List<string> { "Carmen & Mike", "Zach" }),
            new Participant("Zach", new List<string> { "zachcolombo@yahoo.com" }, new List<string> { "Ori & Paul", "Carmen & Mike" }),
        };

        static void Main(string[] args)
        {
            // Step 1: Shuffle participants & Assign recipients
            bool assignmentSuccessful;
            List<Participant> shuffledParticipants;
            do
            {
                assignmentSuccessful = Utils.AssignRecipients(Program.participants, out shuffledParticipants);
            } while (!assignmentSuccessful);    // Lazy check -- retry until a valid assignment is made
            
            // Step 2: Send emails
            Utils.SendParticipationEmail(shuffledParticipants);
        }
    }
}
