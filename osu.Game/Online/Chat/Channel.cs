// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Framework.Lists;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class Channel
    {
        public const int MAX_HISTORY = 300;

        [JsonProperty(@"name")]
        public string Name;

        [JsonProperty(@"description")]
        public string Topic;

        [JsonProperty(@"type")]
        public string Type;

        [JsonProperty(@"channel_id")]
        public long Id;

        [JsonConstructor]
        public Channel()
        {
        }

        /// <summary>
        /// Contructs a private channel
        /// TODO this class needs to be serialized from something like channels/private, instead of creating from a contructor
        /// </summary>
        /// <param name="user">The user </param>
        public Channel(User user)
        {
            Target = TargetType.User;
            Name = user.Username;
            Id = user.Id;
            JoinedUsers.Add(user);
        }

        /// <summary>
        /// Contains every joined user except yourself
        /// </summary>
        public ObservableCollection<User> JoinedUsers = new ObservableCollection<User>();
        public readonly SortedList<Message> Messages = new SortedList<Message>(Comparer<Message>.Default);
        private readonly List<LocalEchoMessage> pendingMessages = new List<LocalEchoMessage>();

        public event Action<IEnumerable<Message>> NewMessagesArrived;
        public event Action<LocalEchoMessage, Message> PendingMessageResolved;
        public event Action<Message> MessageRemoved;

        public Bindable<bool> Joined = new Bindable<bool>();
        public TargetType Target { get; set; }
        public bool ReadOnly { get; set; }

        public void AddLocalEcho(LocalEchoMessage message)
        {
            pendingMessages.Add(message);
            Messages.Add(message);

            NewMessagesArrived?.Invoke(new[] { message });
        }

        public void AddNewMessages(params Message[] messages)
        {
            messages = messages.Except(Messages).ToArray();

            Messages.AddRange(messages);

            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);
        }

        /// <summary>
        /// Replace or remove a message from the channel.
        /// </summary>
        /// <param name="echo">The local echo message (client-side).</param>
        /// <param name="final">The response message, or null if the message became invalid.</param>
        public void ReplaceMessage(LocalEchoMessage echo, Message final)
        {
            if (!pendingMessages.Remove(echo))
                throw new InvalidOperationException("Attempted to remove echo that wasn't present");

            Messages.Remove(echo);

            if (final == null)
            {
                MessageRemoved?.Invoke(echo);
                return;
            }

            if (Messages.Contains(final))
            {
                // message already inserted, so let's throw away this update.
                // we may want to handle this better in the future, but for the time being api requests are single-threaded so order is assumed.
                MessageRemoved?.Invoke(echo);
                return;
            }

            Messages.Add(final);
            PendingMessageResolved?.Invoke(echo, final);
        }

        private void purgeOldMessages()
        {
            // never purge local echos
            int messageCount = Messages.Count - pendingMessages.Count;
            if (messageCount > MAX_HISTORY)
                Messages.RemoveRange(0, messageCount - MAX_HISTORY);
        }

        public override string ToString() => Name;
    }
}
