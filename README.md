A lite password manager project I built. It's a C# .net / WPF / SQLite stack. It uses AES 128 bit encryption to store the Master password as well as the other passwords encrypted within the database.
The key used to encrypt/decrypt is application specific and can be changed in the source code here | https://github.com/srsherman7/PassThis/blob/cf078d26d12cee757adaff870b940fd1194e771f/PassThis/MainWindow.xaml.cs#L16
As of right now it's a simple store encrypted. decrypt when called and copy to clipboard workflow. I am researching adding shortcut keys for future builds.

Some roadblocks to overcome were:

• Database locking - Updated Journal mode to WAL (Write Ahead Logging) which resolved those issues. 
• Connections staying open - moved close() command within the if/else blocks so it would execute based on logic flow.
• Error Handling - Created an errorhandling class and wrapped sqlite operations using a delgate for use as a global try/catch handler.

Build package
https://github.com/srsherman7/PassThis/blob/c854ea870ab05ea0e9f1195f0f80c00b6566f846/PassThisBuild.zip

![image](https://github.com/user-attachments/assets/978e76b3-2eb7-4bae-9191-4c8f9a1c748f)

![image](https://github.com/user-attachments/assets/738273e9-8ecb-4282-bc4c-63c31311348b)

![image](https://github.com/user-attachments/assets/8cc29f04-5630-421d-951f-79328f21e95c)

![image](https://github.com/user-attachments/assets/86f5bc4a-db45-448a-a9ee-c49e3ff23177)

![image](https://github.com/user-attachments/assets/c14eb1b1-9437-49a4-ab6d-ee68afd8f70b)


