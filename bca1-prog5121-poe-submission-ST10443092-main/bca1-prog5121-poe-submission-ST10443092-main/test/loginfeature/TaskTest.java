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
public class TaskTest {
    
    public TaskTest() {
    }

    @Test
    public void descriptionTrue() {
       
        boolean expected = true;//set to true because program expects data from user to be treu(Under 50 characters)
        String testdata = "Create Login to Authenticate users";//the test should only pass if description is 50 or less characters
        boolean actual = Task.checkDescription(testdata);
        assertEquals(actual,expected);
        
   
    }
     
        
    
    @Test
     public void taskID() {
       
       String taskname = "Login Feature";
       String surname = "Robyn";
       String expected = "LO:1:BYN";
       String actual = Task.createTaskID(surname, taskname);
       assertEquals(actual, expected);
        
    }
     @Test
     public void taskID2() {
       
       String taskname = "Add task Feature";
       String surname = "Mike";
       String expected = "AD:0:IKE";
       String actual = Task.createTaskID(surname, taskname);
       assertEquals(actual, expected);
        
    }
     @Test
     public void taskDration() {
      int sum = 10+12+55+11+1;
      int expected = 89;
        Task.taskDuration(sum);
       int actual = Task.totalHours();
      assertEquals(expected,actual);
        
    }
     
    
    
    
}
