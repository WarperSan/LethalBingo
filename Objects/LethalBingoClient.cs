using System.Net.WebSockets;
using BingoAPI;

namespace LethalBingo.Objects;

// ReSharper disable once ClassNeverInstantiated.Global
public class LethalBingoClient : BingoClient
{
    public LethalBingoClient(ClientWebSocket socket, bool isCreator) : base(socket, isCreator)
    {
    }
}