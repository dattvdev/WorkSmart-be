using Microsoft.AspNetCore.SignalR;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IPersonalMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessageHub(
            IPersonalMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task SendMessage(PersonalMessage message)
        {
            // Validate message
           /* var sender = await _userRepository.GetByIdAsync(message.SenderID);
            var receiver = await _userRepository.GetByIdAsync(message.ReceiverID);

            if (sender == null || receiver == null)s
            {
                throw new HubException("Invalid sender or receiver");
            }
*/
            // Lưu tin nhắn
            message.CreatedAt = DateTime.UtcNow;
            await _messageRepository.AddAsync(message);

            // Gửi tin nhắn realtime
            await Clients.User(message.ReceiverID.ToString())
                .SendAsync("ReceiveMessage", message);
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _messageRepository.UpdateAsync(message);

                // Thông báo cho sender biết tin nhắn đã được đọc
                await Clients.User(message.SenderID.ToString())
                    .SendAsync("MessageReadConfirmation", messageId);
            }
        }

        public async Task<IEnumerable<PersonalMessage>> GetConversationHistory(int senderId, int receiverId)
        {
            return await _messageRepository
                .GetConversationMessagesAsync(senderId, receiverId);
        }
    }
}
