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
public class TaskIT {
    
    public TaskIT() {
    }

    @Test
    public void developer() {
       String expected = Task.developerarray();
       String actual = "Mike Smith" + "\n" + "Edward Harrison" + "\n" + "Samantha Paulson" + "\n" + "Glenda Oberholzer";
       assertEquals(expected,actual);
    }
     @Test
    public void duration() {
       int hours = 11;
        String expected = "Developer: Glenda Oberholzer Duration: 11";
          String actual = Task.durationtest();
       assertEquals(expected,actual);
    }
      @Test
    public void tasksearch() {
        String expected = "Task Details:"+"\n"+"\n"+"Task Name: Create Login"+"\n"+"Developer: Mike Smith";
       String searchTaskName = "Create Login";
       String actual = Task.taskSearch(searchTaskName);
       assertEquals(expected,actual);
       
    }
     @Test

     public void developerSearch() {
       String expected = "Developer Details:" + "\n"+"\n"+"Task Name: Create Reports"+"\n"+"Status: Done";
       String searchDeveloperName = "Samantha Paulson";
       String actual = Task.developerSearch(searchDeveloperName);
       assertEquals(expected,actual);
       
    }
      @Test

     public void deletetask() {
       String expected = "Task Create Reports has been successfully deleted";
       String searchDeleteName = "Create Reports";
       String actual = Task.deleteTask(searchDeleteName);
       assertEquals(expected,actual);
       
    }
    
}
