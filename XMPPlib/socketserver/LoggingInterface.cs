/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Runtime.Serialization;

namespace xmedianet.socketserver
{
   /// <summary>
   /// The importance of the message.  The server will only report messages that are as or more important
   /// than the importance requested by the client.
   /// </summary>
   public enum MessageImportance
   {
      Lowest = 0,
      Low = 1,
      MediumLow = 2,
      Medium = 3,
      MediumHigh = 4,
      High = 5,
      Highest = 6,
   }


   public interface ILogInterface
   {
      ///  Functions to log messages
      void LogMessage(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogWarning(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogError(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);

      void LogMessage(string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogWarning(string strGuid, MessageImportance importance, string strMessage, params object [] msgparams);
      void LogError(string strGuid, MessageImportance importance, string strMessage, params object[] msgparams);

      void ClearLog();
   }

   public class ConsoleLogClient : ILogInterface
   {
       public ConsoleLogClient()
       {
       }

       public void ClearLog()
       {

       }

       public void LogError(string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }

       public void LogError(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }

       public void LogMessage(string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }

       public void LogMessage(string strcategory, string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }


       public void LogWarning(string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }

       public void LogWarning(string strCateogry, string strGuid, MessageImportance importance, string strMessage, params object[] msgparams)
       {
           Console.WriteLine(strMessage, msgparams);
       }

       public MessageImportance MinimumImportance
       {
           get
           {
               return MessageImportance.Lowest;
           }
           set
           {
           }
       }
   }



}
