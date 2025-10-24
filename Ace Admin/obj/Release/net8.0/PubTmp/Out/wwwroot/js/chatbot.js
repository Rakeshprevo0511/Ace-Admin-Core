class AIChatbot {
    constructor() {
        this.chatWindow = document.getElementById('aiChatWindow');
        this.chatToggle = document.getElementById('aiChatToggle');
        this.chatClose = document.getElementById('aiChatClose');
        this.chatMessages = document.getElementById('aiChatMessages');
        this.chatInput = document.getElementById('aiChatInput');
        this.chatSend = document.getElementById('aiChatSend');
        this.chatBadge = document.getElementById('aiChatBadge');
        this.suggestions = document.querySelectorAll('.ai-suggestion-chip');

        this.isTyping = false;
        this.messageHistory = [];

        this.init();
    }

    init() {
        // Set initial timestamp (optional)
        const botTime = document.getElementById('botMessageTime');
        if (botTime) botTime.textContent = this.getTimeString();

        // Toggle chat window
        if (this.chatToggle) this.chatToggle.addEventListener('click', () => this.toggleChat());
        if (this.chatClose) this.chatClose.addEventListener('click', () => this.closeChat());

        // Send message on button or Enter
        if (this.chatSend) this.chatSend.addEventListener('click', () => this.sendMessage());
        if (this.chatInput) {
            this.chatInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.sendMessage();
            });
        }

        // Suggestion chips
        this.suggestions.forEach(chip => {
            chip.addEventListener('click', () => {
                const suggestion = chip.getAttribute('data-suggestion');
                this.chatInput.value = suggestion;
                this.sendMessage();
            });
        });
    }

    toggleChat() {
        this.chatWindow.classList.toggle('active');
        if (this.chatWindow.classList.contains('active')) {
            if (this.chatInput) this.chatInput.focus();
            if (this.chatBadge) this.chatBadge.style.display = 'none';
        }
    }

    closeChat() {
        this.chatWindow.classList.remove('active');
    }

    getTimeString() {
        const now = new Date();
        return now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    }

    async sendMessage() {
        const message = this.chatInput.value.trim();
        if (!message || this.isTyping) return;

        // Add user message
        this.addMessage(message, 'user');
        this.chatInput.value = '';

        // Show typing indicator
        this.showTyping();

        // Call backend API via jQuery AJAX
        try {
            const response = await this.callAIAPI(message);
            this.hideTyping();
            this.addMessage(response, 'bot');
        } catch (error) {
            this.hideTyping();
            this.addMessage('Sorry, I encountered an error. Please try again.', 'bot');
            console.error('AI API Error:', error);
        }
    }

    callAIAPI(message) {
        const self = this;
        const token = localStorage.getItem('employeeToken');

        return new Promise(function (resolve, reject) {
            $.ajax({
                url: '/api/ChatBot/ChatFree', // MVC API endpoint
                type: 'POST',
                contentType: 'application/json',
                headers: {
                    'Authorization': 'Bearer ' + token
                },
                data: JSON.stringify({
                    message: message,
                    history: self.messageHistory
                }),
                success: function (response) {
                    if (response.success) {
                        resolve(response.response);
                    } else {
                        reject(response.message || 'API returned an error');
                    }
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 401) {
                        alert('Unauthorized. Please log in again.');
                        window.location.href = '/Home/Login';
                    } else {
                        reject(error || 'Server error');
                    }
                }
            });
        });
    }

    addMessage(text, type) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `ai-chat-message ${type}`;

        const avatarDiv = document.createElement('div');
        avatarDiv.className = `ai-message-avatar ${type}`;
        avatarDiv.innerHTML = type === 'bot'
            ? '<i class="fas fa-robot"></i>'
            : '<i class="fas fa-user"></i>';

        const contentDiv = document.createElement('div');
        contentDiv.className = 'ai-message-content';

        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = `ai-message-bubble ${type}`;
        bubbleDiv.textContent = text;

        const timeDiv = document.createElement('div');
        timeDiv.className = 'ai-message-time';
        timeDiv.textContent = this.getTimeString();

        contentDiv.appendChild(bubbleDiv);
        contentDiv.appendChild(timeDiv);
        messageDiv.appendChild(avatarDiv);
        messageDiv.appendChild(contentDiv);

        this.chatMessages.appendChild(messageDiv);
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;

        // Store in history
        this.messageHistory.push({ role: type === 'user' ? 'user' : 'assistant', content: text });
    }

    showTyping() {
        this.isTyping = true;
        const typingDiv = document.createElement('div');
        typingDiv.className = 'ai-chat-message bot';
        typingDiv.id = 'typingIndicator';

        typingDiv.innerHTML = `
                    <div class="ai-message-avatar bot">
                        <i class="fas fa-robot"></i>
                    </div>
                    <div class="ai-message-content">
                        <div class="ai-message-bubble bot">
                            <div class="ai-chat-typing">
                                <div class="ai-typing-dot"></div>
                                <div class="ai-typing-dot"></div>
                                <div class="ai-typing-dot"></div>
                            </div>
                        </div>
                    </div>
                `;

        this.chatMessages.appendChild(typingDiv);
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }

    hideTyping() {
        this.isTyping = false;
        const typingIndicator = document.getElementById('typingIndicator');
        if (typingIndicator) typingIndicator.remove();
    }
}

// Initialize chatbot when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    window.aiChatbot = new AIChatbot();
});