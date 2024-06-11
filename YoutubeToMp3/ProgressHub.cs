using Microsoft.AspNet.SignalR; // Import the SignalR namespace

namespace YoutubeToMp3
{
    public class ProgressHub : Hub // Define a class named ProgressHub that inherits from the SignalR Hub class
    {
        // This class represents a SignalR hub, which acts as a communication endpoint
        // Clients can connect to this hub to send and receive messages in real-time
        // The ProgressHub class can contain methods to handle different types of client requests and events
        // For example, you can define methods to send messages to clients or handle client connections and disconnections
    }
}
