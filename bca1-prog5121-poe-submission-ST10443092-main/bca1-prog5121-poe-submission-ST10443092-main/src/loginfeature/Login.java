/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package loginfeature;

/**
 *
 * @author Qhawe General
 */
import java.util.regex.Matcher;//Piwowarek 2024
import java.util.regex.Pattern;//Piwowarek 2024
import javax.swing.JOptionPane;
public class Login 
{
 
    public static boolean checkUserName(String username)
    {
    
     boolean containsUnderscore = username.contains("_");
     boolean length = username.length()<=5;
      
     return  containsUnderscore && length;
        
     
}
    
   

    public static boolean checkPassword(String password)
    {
       boolean containsCapital =  checkForCapitalLetter(password);
       boolean length = password.length()>=8;  
       boolean numbers = CheckForNumbers(password);
       boolean characters = checkForSpecialCharacters(password);
       return length && numbers && containsCapital && characters;
      
    }      
         
    public static boolean checkForCapitalLetter(String input) {
      return !input.equals(input.toLowerCase()); 
    }
    public static boolean CheckForNumbers(String input)
    {
     return input.matches(".*\\d.*"); //Azhrioun 2024
    }  
    public static boolean checkForSpecialCharacters(String input)
    {
        String special = "^(?=.*[^a-zA-Z0-9])(?=.*\\d).+$";//Kim 2020
        Pattern pattern = Pattern.compile(special);//Piwowarek 2024
        Matcher matcher = pattern.matcher(input);//Piwowarek 2024
        return matcher.matches();
    }
    public static String registerUser ()
    { 
        String name = JOptionPane.showInputDialog(null,"Enter name and surname");
       String User = JOptionPane.showInputDialog(null,"Create Username (Less than 5 characters and an underscore)");
     
        if(Login.checkUserName(User))
            JOptionPane.showMessageDialog(null, " Username Correctly formatted" );
        else
            JOptionPane.showMessageDialog(null, " Username not correctly formatted. Ensure username is no more than 5 characters and has an underscore");
       
       String Pass = JOptionPane.showInputDialog(null,"Create Password (More than 8 characters including atleast 1 digit and a special character)");
        
        if(Login.checkPassword(Pass))
            JOptionPane.showMessageDialog(null, " Password Succesfully Captured");
         else
            JOptionPane.showMessageDialog(null, "Password not correctly formatted. Please Make sure Password has 8 or more character, atleast a number and a special chararcter");
        if(Login.checkPassword(Pass) && Login.checkUserName(User))
            JOptionPane.showMessageDialog(null, "Login Details succesfully Captured");
         else
              JOptionPane.showMessageDialog(null, "Login Details Uuccesfully Captured. Please try again and make sure you have met criteria for password and/or username");
          JOptionPane.showMessageDialog(null, "Welcome " + name); 
        return User ;
    } 
     
    
   
    public static boolean loginUser(String username, String password)
    {
       
        Login.checkUserName(username);
        Login.checkPassword(password);
             
       return true;

}
    public static boolean loginuser(String username, String password)
    {
      
       Login.checkUserName(username);
        Login.checkPassword(password);
             
       return true;

}
              
              
             
       
            
    

        
   
   
       
   public static void returnLoginStatus(String username, String password) {
   
       if (Login.loginUser(username, password) && Login.checkPassword(password) && Login.checkUserName(username)) {
            JOptionPane.showMessageDialog(null, "Welcome back, good to see you" );
            
        } else {
            JOptionPane.showMessageDialog(null, "Login Failed incorrect details");
          
              } 
     
       
   
   } 
   public static void login()
   {
      int choice = JOptionPane.showOptionDialog( //BabyBurger 2014
            
        null,
 "Register your credentials",
    "Register",
    JOptionPane.YES_NO_CANCEL_OPTION,
   JOptionPane.QUESTION_MESSAGE,
    null,
      new String[]{ "Register", "Cancel"},
                
     "Login");
    switch(choice){
 case JOptionPane.YES_OPTION: 
   
     Login.registerUser();
  break;
    case JOptionPane.NO_OPTION: 
   // Login.registerUser();
         System.exit(0);
  break;
    case JOptionPane.CANCEL_OPTION: 
         System.exit(0);
    
        
       
               
    }
   
      int login = JOptionPane.showOptionDialog(
  null,
     "Login",
     "Login",
   JOptionPane.YES_NO_CANCEL_OPTION,
    JOptionPane.QUESTION_MESSAGE,
      null,
    new String[]{ "login", "Cancel"},
               "Login");
  choice = JOptionPane.showConfirmDialog(null, "Would you like to login?", "Login", JOptionPane.YES_NO_OPTION);
    switch (choice) {
        case JOptionPane.YES_OPTION:
       String Username = JOptionPane.showInputDialog(null," Enter your Username");
    String Password = JOptionPane.showInputDialog(null,"Enter your Password");
   Login.loginUser(Username, Password);
      
boolean x = false;

while (!x) {
    if (Login.loginUser(Username, Password)) { 
        Login.returnLoginStatus(Username, Password);
        x = true; 
    } else {
        JOptionPane.showMessageDialog(null, "Login Failed");
       Username = JOptionPane.showInputDialog(null," Enter your Username");
     Password = JOptionPane.showInputDialog(null,"Enter your Password");
   Login.loginUser(Username, Password);
    }
}   
                
     case JOptionPane.NO_OPTION: 
        //System.exit(0);    
      break;
     case JOptionPane.CANCEL_OPTION: 
    break ;
     default:
         break;
    }  
   }
   
    }
     
    
      
       
  
            
        
        
      
           
    


//Piwowarek, G. 2024. A Guide To Java Regualr Expressions
// Available at: https://www.baeldung.com/regular-expressions-java#:~:text=We'll%20first%20create%20a,want%20to%20check%20for%20matches[Accessed:04 April 2024]


//Kim, M, Y. 2020. Java regex check non-alphanumeric string
//Available at https://mkyong.com/regular-expressions/java-regex-check-non-alphanumeric-string/#:~:text=The%20Alphanumericals%20are%20a%20combination,%5D%2B%20to%20matches%20alphanumeric%20characters.[Accessed: 31 March 2024]
    
 // OpenAI. 2022. Chat-GPT (Version GPT-3.5). [Large language model]. 
//Available at: https://chat.openai.com/ [Accessed: 31 March 2024].

//Farrel, J 2019.Java Programming.9th edition.2019
//Course Technology, Cengage Learning

//Azhrioun, A. 2024 Check if a string has a number value Java
//Available at https://www.baeldung.com/java-string-number-presence [Accessed 02 April 2024]