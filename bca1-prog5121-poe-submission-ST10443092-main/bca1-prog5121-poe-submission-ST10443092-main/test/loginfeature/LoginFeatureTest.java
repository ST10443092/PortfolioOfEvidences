/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/UnitTests/JUnit4TestClass.java to edit this template
 */
package loginfeature;

import org.junit.Test;
import static org.junit.Assert.*;

/**
 *
 * @author lab_services_student
 */
public class LoginFeatureTest {
    
    public LoginFeatureTest() {
    }

    @Test

    public void testUsername() 
    {
      boolean expected = true; 
     String username = "kyl_1";
     boolean actual = Login.checkUserName(username);
       assertEquals(expected, actual);   
}
    
    @Test
      public void testUsernameWrong() 
    {
      boolean expected = false; 
     String username = "kyle!!!!!";
     boolean actual = Login.checkUserName(username);
       assertEquals(expected, actual); 
         
    }
      @Test
    public void testPassword()
    {
        boolean expected = true;
        String password = "Ch&&sec@ke99";
        boolean actual = Login.checkPassword(password);
        assertEquals(expected, actual);
        
    }@Test
    public void testPasswordWrong()
    {
        boolean expected = false;
        String password = "password";
        boolean actual = Login.checkPassword(password);
        assertEquals(expected, actual);
        
    }
   
    
  @Test
        public void testLogin()
        {
            String password = "Ch&&sec@ke99";
            String username = "kyl_1";
           assertTrue(Login.loginUser(username, password));
            
        }
        @Test
        public void testPasswordTrue()
        {
       
        String expected = "Ch&&sec@ke99";
        boolean actual = Login.checkPassword(expected);
        assertTrue(expected, actual);
        
}
         
        


        @Test
        public void testUernameTrue()
        {
        String expected = "kyl_l";
        boolean actual = Login.checkUserName(expected);
        assertTrue(expected, actual);
        
}
         @Test
        public void testUernameFalse()
        {
        String expected = "kyle!!!!!!";
        boolean actual = Login.checkUserName(expected);
        assertFalse(expected, actual);
        
}
       @Test
        public void testPasswordFalse()
        {
        String expected = "password";
        boolean actual = Login.checkUserName(expected);
        assertFalse(expected, actual);
        
}  
}
