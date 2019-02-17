using System.IO;

namespace telnot
{
    static class TelnetNegociator
    {
        public static string Negociate(Stream stream)
        {
            string text = "";
            int input = stream.ReadByte();
            switch (input)
            {
                case -1:
                    break;
                case TelnetCodes.IAC:
                    int verb = stream.ReadByte();
                    if (verb == -1) break;
                    switch (verb)
                    {
                        case TelnetCodes.IAC:
                            text += verb;
                            break;
                        case TelnetCodes.DO:
                        case TelnetCodes.DONT:
                        case TelnetCodes.WILL:
                        case TelnetCodes.WONT:
                            int option = stream.ReadByte();
                            if (option == -1) break;
                            stream.WriteByte(TelnetCodes.IAC);
                            if (option == TelnetCodes.SGA)
                                stream.WriteByte(verb == TelnetCodes.DO ? TelnetCodes.WILL : TelnetCodes.DO);
                            else
                                stream.WriteByte(verb == TelnetCodes.DO ? TelnetCodes.WONT : TelnetCodes.DONT);
                            stream.WriteByte((byte)option);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    text += (char)input;
                    break;
            }
            return text;
        }

    }
}
