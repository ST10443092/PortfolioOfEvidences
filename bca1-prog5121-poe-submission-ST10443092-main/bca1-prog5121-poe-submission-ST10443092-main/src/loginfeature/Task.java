/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package loginfeature;

/**
 *
 * @author lab_services_student
 */
import javax.swing.JOptionPane;
//import java.util.Random;
import java.util.ArrayList;
import java.util.List;
public class Task {
    private static 
          int taskN = 0;
            private static  int hours;
            //private static  int details [] = new int[5];
    // private static   String[] developer = new String[5];
    //private static  String taskname [] = new String[5];
    // private static String taskID [] = new String [taskNo];
    // private static int duration [] = new int[5];
      private static List<String> Status;
    private static List<String> developer;
    private static List<String> taskname;
    private static List<Integer> duration;
    private static List<Integer> details;

    static {
        Status = new ArrayList<>();
        developer = new ArrayList<>();
        taskname = new ArrayList<>();
        duration = new ArrayList<>();
        details = new ArrayList<>();

        developer.add("Mike Smith");
        developer.add("Edward Harrison");
        developer.add("Samantha Paulson");
        developer.add("Glenda Oberholzer");

        Status.add("To Do");
        Status.add("Doing");
        Status.add("Done");
        Status.add("To do");

        duration.add(5);
        duration.add(8);
        duration.add(2);
        duration.add(11);

        taskname.add("Create Login");
        taskname.add("Create Add Features");
        taskname.add("Create Reports");
        taskname.add("Add Arrays");
    }

    public Task() {
        // Constructor remains empty or can contain other instance-specific initializations
    }

    public static void done() {
        if (Status == null) {
            Status = new ArrayList<>();
        }
        if (developer == null) {
            developer = new ArrayList<>();
        }
        if (taskname == null) {
            taskname = new ArrayList<>();
        }
        if (duration == null) {
            duration = new ArrayList<>();
        }
        if (details == null) {
            details = new ArrayList<>();
        }
        

    }

    public static boolean  checkTaskDescription(String desc)
    {
        
     boolean length = desc.length()<=50;
      
    return length;
        
    }
public static void outputTrue()
{
    JOptionPane.showMessageDialog(null, "Description Succesfully captured");
}

public static void outputfalse()
{
     JOptionPane.showMessageDialog(null, "Please make sure task description is less than 50 characters");
}
    public static  boolean checkDescription(String desc)
    {
       boolean length = desc.length()<=50;
      
    
       
        
      
        return length;
    }
    
       
                
    public static int taskNO()
    {
         
       
            return taskN++;
        
    
    }
    public  static int taskDuration (int hour)
    {
        hours = hour;//int  hours = Integer.parseInt(JOptionPane.showInputDialog(null,"Enter hours for task"));
       return hours;
    }
    public static int totalHours()
    {
     int duration = + Task.taskDuration(hours);
     return duration;
        
    }
        public static String createTaskID(String surname, String taskname)
        {
            String tName = taskname.substring(0, 2);
         String Sname = surname.substring(surname.length() - 3);//Farell 2019
          return tName.toUpperCase() + ":" + Task.taskNO()+":" + Sname.toUpperCase();
        }
        public static  String printTaskDetails(String tName,String Description,String name, String surname, String status, int hours )
                
        {
            
           JOptionPane.showMessageDialog(null, "The details of your task are as follows" + "\n" + "Task name :"+ tName + "\n" + "Task Number :"+Task.taskNO() + "\n" +"Description :"+ Description + "\n" +"Developer details :"+ name +" " + surname + "\n" +"Time for task :"+ hours + "hrs" +"\n" +"Task ID :"+Task.createTaskID(surname, tName) + "\n"+ "Task Status :"+ status      );
            return "Welcome";    
        }
   
    
        public static String developerarray()
        {
            return developer.get(0)+ "\n"+ developer.get(1)+"\n"+developer.get(2)+"\n"+developer.get(3);
        }
         public static void doneSearch()
         {
       // int x = 0;
            boolean foundCompleted = false;

            
        for (int x = 0; x < 4; x++) {
        if (Status.get(x).equalsIgnoreCase("done")) {
            foundCompleted = true;
            JOptionPane.showMessageDialog(null, "Completed Tasks" + "\n" + "\n" +
                "Developer: " + developer.get(x) + "\n" +
                "Task Name: " + taskname.get(x) + "\n" +
                "Duration: " + duration.get(x));
        }
        
    }
//} else {
//    System.out.println("Error: Lists are not of the same size.");
//}

                if (foundCompleted = false) {
        JOptionPane.showMessageDialog(null, "There are no completed tasks");
    }
         }
         public static void report(){
             
             int k;
               for (int i = 0; i < 4; i++) {
                  k=i+1;
                   JOptionPane.showMessageDialog(null, "Report for task "+k+"\n"+"\n"
                           + "Developer:  "+developer.get(i)+"\n"+
                           "Task:  "+taskname.get(i)+"\n"+
                           "Duration:  "+duration.get(i)+"\n"+
                           "Status:  "+Status.get(i));
               }
             
    
            }
         public static  void  durationdisplay()
         {
           int maxDuration = 0;
             maxDuration = duration.get(0);
        for (int i = 1; i < 4; i++) {
            if (duration.get(i) > maxDuration) {
                maxDuration = duration.get(i);
            }
        }
     
           String result = "";
          for (int i = 0; i < 4; i++) {
            if (duration.get(i) == maxDuration) {
               JOptionPane.showMessageDialog(null,"Developer: " + developer.get(i) +  " Duration: " + duration.get(i));
            }
        }
         
         }
         public static String durationtest()
          {
           int maxDuration = 0;
             maxDuration = duration.get(0);
        for (int i = 1; i < 4; i++) {
            if (duration.get(i) > maxDuration) {
                maxDuration = duration.get(i);
            }
        }
     
           String result = "";
          StringBuilder message = new StringBuilder();
    for (int i = 0; i < developer.size(); i++) {
        if (duration.get(i) == maxDuration) {
            message.append("Developer: ").append(developer.get(i)).append(" Duration: ").append(duration.get(i));
        }
    }
 
    return message.toString();
        
          }     
        public static String  taskSearch(String searchTaskName)
        {
          String result = "";
        boolean found = false;
        for (int i = 0; i <4; i++) {
            StringBuilder message = new StringBuilder();
            if (taskname.get(i).equalsIgnoreCase(searchTaskName)) {
                found = true;
                //StringBuilder message = new StringBuilder();
               message.append("Task Details:\n\n")
       .append("Task Name: ").append(taskname.get(i)).append("\n")
       .append("Developer: ").append(developer.get(i));
        
              JOptionPane.showMessageDialog(null, message.toString());
              
               result = message.toString();
              if(!found) 
            JOptionPane.showMessageDialog(null, "No task found with the name: " + searchTaskName);  
        
        
            }
        }
         return result;
        }
        public static String developerSearch(String developerSearch)
         {
              
             String result =""; 
                     boolean found = false;
              StringBuilder message = new StringBuilder();
       for (int i = 0; i < 4; i++) {
    if (developer.get(i).equalsIgnoreCase(developerSearch)) {
        found = true;
        message.append("Developer Details:\n\n")
                      .append("Task Name: ").append(taskname.get(i)).append("\n")
                      .append("Status: ").append(Status.get(i));
        
        JOptionPane.showMessageDialog(null, message.toString());
        result = message.toString();
    }
}

        if(!found)
            JOptionPane.showMessageDialog(null, "No developer  found with the name: " + developerSearch); 
        return result;
         }
         public static String deleteTask(String taskDelete)
         {
             String result = "";
             boolean found = false;
             StringBuilder message = new StringBuilder();
        for (int i = taskname.size() - 1; i >= 0; i--) {
    if (taskname.get(i).equalsIgnoreCase(taskDelete)) {
        found = true;
        
      
        taskname.remove(i); 
        developer.remove(i);
        Status.remove(i);
        duration.remove(i);

        
        message.setLength(0); 
        message.append("Task ").append(taskDelete).append(" has been successfully deleted");
        
        
        // Display the message
        JOptionPane.showMessageDialog(null, message.toString());
        result = message.toString();
       
    }
   
}

        if(!found)
            JOptionPane.showMessageDialog(null,   taskDelete + "Could not be deleted as it doesnt exist");  
         return result;
         }
         
    public static void recordTask()
    {
   
String desc = "";
    
        JOptionPane.showMessageDialog(null,"Welcome to EasyKanBan");
       
        
        Object[] options = {"Add Task", "Show Report", "Quit"};
        int choice2;
        
       do{ 
         choice2 = JOptionPane.showOptionDialog(null,
                "What would you like to do?",
                "Task Manager",
                JOptionPane.YES_NO_CANCEL_OPTION,
                JOptionPane.QUESTION_MESSAGE,
                null,
                options,
                options[0]);

        
        switch (choice2) {
            case 0:
               
                JOptionPane.showMessageDialog(null, "Add Task selected");
                
                   String name = "";
    String Surname = "";
   
    String taskName = "";
     // int taskNo = 0;
     // name = JOptionPane.showInputDialog(null,"Please Enter your name");
    // Surname = JOptionPane.showInputDialog(null,"Please Enter Surname");
   
    String status = "";
       int taskNo = Integer.parseInt(JOptionPane.showInputDialog(null,"Please Enter amount of tasks to record"));


   int Hours = 0;
  
  int hours = 0;
  int k = 0;
     int display[]= new int[taskNo]
             ;     
   while(k<taskNo){
       name = JOptionPane.showInputDialog(null,"Please Enter your name");
      Surname = JOptionPane.showInputDialog(null,"Please Enter Surname");
      //developer.set(k,name );
         
       taskName = JOptionPane.showInputDialog(null,"Please Enter Task name for task " + k);
      // taskname.set(k,taskName);
      // taskID[k]= Task.createTaskID(Surname, taskName);
     desc = JOptionPane.showInputDialog(null,"Please Enter the description of your task (Must be 50 or less characters)");
    //Task.checkDescription(desc);
    
     if (Task.checkDescription(desc)) {
            Task.outputTrue();
        } else {
            Task.outputfalse();
        }
     
     status = JOptionPane.showInputDialog(null,"Please Enter task status" + "\n" +"A     To Do" +"\n" + "B     Done" +"\n" + "C     Doing");
     while(!status.equalsIgnoreCase("A" )&& !status.equalsIgnoreCase("B" ) && !status.equalsIgnoreCase("C" )){
         JOptionPane.showMessageDialog(null, "Please choose an valid option");
       status = JOptionPane.showInputDialog(null,"Please Enter task status" + "\n" +"A     To Do" +"\n" + "B     Done" +"\n" + "C     Doing");
   }
     if(status.equalsIgnoreCase("A"))
         status="To Do";
     else if(status.equalsIgnoreCase("B"))
         status = "Done";
     else if(status.equalsIgnoreCase("C"))
         status = "Doing";
     else
         JOptionPane.showMessageDialog(null, "Please choose an valid option");
     // Status.set(k,status);
    
       Hours = Integer.parseInt(JOptionPane.showInputDialog(null,"Please enter hours for tasks"));
      display[k]=Hours;
// duration.set(k,Hours);
      // details.set(k,Hours);  
 Task.taskDuration(Hours);
     //Task.taskDuration(Hours);
  JOptionPane.showMessageDialog(null, "Task recorded succesfully. Continue to view task details" );
 // obj.taskDuration(Hours);
 printTaskDetails(taskName, desc, name, Surname, status, Hours);
   k++;
   }
  
   
   int sum = 0;
    for(int x = 0;x<taskNo;x++){
             sum += display[x];
        }
       JOptionPane.showMessageDialog(null,"total duration " + sum);
   System.exit(0);
   break;
    //System.exit(0);
              
                //break;
            case 1:
                 String delete = "";
                 boolean loop = false;
              
               String option = JOptionPane.showInputDialog(null,"Please select an option" + "\n" +"\n" 
                       + "\n" +  "A     Completed Tasks"+ "         D     Search by developer" +
                       "\n" +"B     Longest duration" + "          E     Delete a task" + "\n" + 
                       "c     Search by task name" + "   F     Display Report" +"\n"
                                + "G        Quit");
               if (option.equalsIgnoreCase("G")){
                     System.exit(0);
               }
               else
                   loop = true;
                   
              
                while(!option.equalsIgnoreCase("A" )&& !option.equalsIgnoreCase("B" ) && !option.equalsIgnoreCase("C" )&& !option.equalsIgnoreCase("D" )&& !option.equalsIgnoreCase("E" )
                        && !option.equalsIgnoreCase("F" )){
         JOptionPane.showMessageDialog(null, "Please choose an valid option");
         
                option = JOptionPane.showInputDialog(null,"Please select an option" + "\n" +"\n" 
                       + "\n" +  "A     Completed Tasks"+ "         D     Search by developer" +
                       "\n" +"B     Longest duration" + "          E     Delete a task" + "\n" + 
                       "c     Search by task name" + "   F     Display Report" );
      
   }
                //int y = 0;
                String searchTaskName = null;
                 String searchDeveloperName = null;
                int maxDuration = 0;
     if(option.equalsIgnoreCase("A")){
          
      
        Task.doneSearch();
        System.exit(0);
     }    
     else if(option.equalsIgnoreCase("B")){
        Task.durationdisplay();
        System.exit(0);
     }
    
     else if(option.equalsIgnoreCase("C")){
        String search = JOptionPane.showInputDialog(null,"Ënter task to search for");
         Task.taskSearch(search);
        System.exit(0);
                }       
        
     
         else if(option.equalsIgnoreCase("D")){
           delete = JOptionPane.showInputDialog(null,"Ënter developer to search for");   
     Task.developerSearch(delete);
     System.exit(0);
         }
         else if(option.equalsIgnoreCase("E")) {
            delete = JOptionPane.showInputDialog(null,"Ënter task to delete"); 
             Task.deleteTask(delete);
           }
      else if(option.equalsIgnoreCase("F")){
          Task.report();
          
      }
                 System.exit(0);
            break;
            case 2:
               
                JOptionPane.showMessageDialog(null, "Quitting application");
                
                //System.exit(0);
                break;
            default:
                
                
              
        } 

   
  
    
    
  
    } while (choice2 != 2);
    }
}

//Paul, J 2021. How to Add leading Zeros to Integers in Java
//Available at https://javarevisited.blogspot.com/2013/02/add-leading-zeros-to-integers-Java-String-left-padding-example-program.html#axzz8a181Oy8H [Accessed 12 May 2024]
  
