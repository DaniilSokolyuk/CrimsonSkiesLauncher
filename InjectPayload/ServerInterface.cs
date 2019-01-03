using System;

namespace InjectPayload
{
    public class ServerInterface : MarshalByRefObject
    {
        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("InjectPayload has injected into process {0}.\r\n", clientPID);
        }

        /// <summary>
        /// Output the message to the console.
        /// </summary>
        /// <param name="fileNames"></param>
        public void ReportMessages(string[] messages)
        {
#if DEBUG
            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
#endif
        }

        public void ReportMessage(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public void ReportException(Exception e)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        int count;
        public void Ping()
        {
#if DEBUG
            // Output token animation to visualise Ping
            var oldTop = Console.CursorTop;
            var oldLeft = Console.CursorLeft;
            Console.CursorVisible = false;

            var chars = "\\|/-";
            Console.SetCursorPosition(Console.WindowWidth - 1, Console.WindowHeight-1);
            Console.Write(chars[count++ % chars.Length]);

            Console.SetCursorPosition(oldLeft, oldTop);
            Console.CursorVisible = true;
#endif
        }
    }
}