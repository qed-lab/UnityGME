using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Enums;
using Mediation.FileIO;
using Mediation.Interfaces;
using Mediation.Planners;
using Mediation.PlanTools;

namespace PlanRecognition {
	public class PlanRecognizer : MonoBehaviour {
		
        public int actionCountWait = 1; // Wait this amount of actions prior to running the plan recognition.
		public int numberOfGoals = -1; // The number of goals in this plan recognition theory / NOTE: This is hard-coded for now.
		[HideInInspector] public string observationLogFilePath; // Path to the observation log file.
		[HideInInspector] public FileInfo observationLogFile; // The observation log file.

        private string domainName = "";
		private Mediator mediator; // The GME Mediator Game Object
		private int optimalPlanLength; // The length of the optimal plan that solves the GME's planning problem.
		private int numberOfDomainOperators; // The number of operators in the domain.
		private Thread planRecognitionThread; // The Thread that takes care of the plan recognition process.
		private int actionCountCache = 0; // The number of actions taken by the player.
		private string parserPath;   // The path of the parser of the observation compiler files.
		private string obsCompilerBatchScriptPath; // The path of the batch script used to compile player observations.
		private Plan recognizedPlan;  // The resulting plan that has been recognized by the plan recognizer.

		// Debug:
		private int planRecognitionRunCount = 0;
		private string planRecognitionLogFilePath;

		// Use this for initialization
		public void Start () {
		
			// Find the GME's Mediator object and record the optimal plan length.
			GameObject mediatorGO = GameObject.Find("Mediator");
			if(mediatorGO != null) {
				this.mediator = mediatorGO.GetComponent<Mediator>();
				this.optimalPlanLength = this.mediator.plan.Steps.Count;
				this.numberOfDomainOperators = this.mediator.domain.Operators.Count;
                this.domainName = this.mediator.domainName;
			}

			// Reset the action count of the PlanRecognizer.
			this.actionCountCache = 0;

			// Set the path to the top directory.
			this.parserPath = Directory.GetParent(Application.dataPath).FullName + "/";
			Parser.path = this.parserPath;

			// Set the path to the observation log file we're going to monitor
			this.observationLogFilePath = (Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\obs.dat").Replace("/", "\\");

			// Register the file infomation of the observation log
			this.observationLogFile = new FileInfo(this.observationLogFilePath);

			// Clear the contents of the log.
			System.IO.File.WriteAllText(this.observationLogFilePath, string.Empty);

			// Set the file information attributes to the present time, to indicate it's a new log.
			DateTime presentTime = DateTime.Now;
			System.IO.File.SetCreationTime(this.observationLogFilePath, presentTime); 
			System.IO.File.SetLastWriteTime(this.observationLogFilePath, presentTime); 

			// Set the observation compiler batch script file path
			this.obsCompilerBatchScriptPath = Parser.GetTopDirectory() + @"\obscompile.bat";

			// Debug:
			this.planRecognitionRunCount = 0;
			string planRecognitionLogFolder = Parser.GetTopDirectory() + @"Benchmarks\" + this.domainName + @"\pr-output\runtime\";
			string[] logFolderFiles = Directory.GetFiles(planRecognitionLogFolder);
			this.planRecognitionLogFilePath = planRecognitionLogFolder + this.domainName + @"_runtime" + logFolderFiles.Length.ToString("D4") + @".dat";
			System.IO.File.WriteAllText(this.planRecognitionLogFilePath, string.Empty);
			System.IO.File.AppendAllText(this.planRecognitionLogFilePath, "domain,numberOfGoals,optimalPlanLength,numberOfOperators,planRecognitionRunCount,playerActionCount,runtime\n");
		}
		
		// Update is called once per frame
		public void Update () {

            // The cache serves as a guard for calling the recognition system
            // infinitely. 
            if(this.actionCountCache != this.mediator.playerLog.Count)
            {
                this.actionCountCache = this.mediator.playerLog.Count;

                if (this.actionCountCache % this.actionCountWait == 0)
                {
                    // Write out the log and kick off the recognition thread.
                    System.IO.File.WriteAllText(this.observationLogFilePath, string.Empty); // Erase old data.
                    List<IOperator> filtered = ObservationFilter.filter(this.mediator.playerLog, FilterMode.WINDOWED);

                    foreach (IOperator playerAction in filtered)
                    {
                        System.IO.File.AppendAllText(this.observationLogFilePath, playerAction + "\n");
                    }

                    // Attempt to recognize the plan.
                    this.planRecognitionThread = new Thread(new ThreadStart(this._RecognizePlan));
                    this.planRecognitionThread.Start();

                    // Debug:
                    this.planRecognitionRunCount++;
                }
            }

			if(this.recognizedPlan != null) {
				print("Recognized plan of length: " + this.recognizedPlan.Steps.Count);
				this.recognizedPlan = null;
			}
		}

		/// <summary>
		/// Carries out the plan recognition process. 
		/// </summary>
		private void _RecognizePlan() {
			DateTime planRecognitionStart = DateTime.Now;
			print("PlanRecognizer._RecognizePlan() has been invoked.");
			this._CompileObservations();
			this._FastDownwardPlan();
			print("PlanRecoginzer._RecognizePlan() has finished!");
			DateTime planRecognitionEnd = DateTime.Now;
			TimeSpan elapsedTime = planRecognitionEnd - planRecognitionStart;
			print(elapsedTime);

			// Debug:
			this._WritePlanRecognitionLogToFile(this.planRecognitionRunCount, this.actionCountCache, elapsedTime);
		}

		/// <summary>
		/// Write the plan recognition performance metrics to a log file.
		/// </summary>
		private void _WritePlanRecognitionLogToFile(int planReconitionRunCount, int actionCount, TimeSpan elapsedTime) {
			// domain,numberOfGoals,optimalPlanLength,numberOfOperators,planRecognitionRunCount,playerActionCount,runtime
			string logLine = this.domainName+","+this.numberOfGoals+","+this.optimalPlanLength +","+this.numberOfDomainOperators+","+ + planReconitionRunCount+","+actionCount+","+elapsedTime.TotalMilliseconds+"\n";
			System.IO.File.AppendAllText(this.planRecognitionLogFilePath, logLine);
		}

		/// <summary>
		/// Compiles the observations of the players actions and creates two 
		/// files in the domain directory: pr-problem.pddl & pr-domain.pddl
		/// </summary>
		private void _CompileObservations() {
			print("PlanRecognizer._CompileObservations() has been invoked.");

			// Start the observation compiler batch file
			ProcessStartInfo obsCompilerBatchScript = new ProcessStartInfo(this.obsCompilerBatchScriptPath);

			// Store the process' arguments
			obsCompilerBatchScript.Arguments = Parser.GetScriptDirectory() + " " + this.domainName + " " + "O1";
			obsCompilerBatchScript.WindowStyle = ProcessWindowStyle.Hidden;
		
			// Start the process and wait for it to be done
			using (Process proc = Process.Start(obsCompilerBatchScript)) {
				proc.WaitForExit();
			}
			print("PlanRecognizer._CompileObservations() has finished!");
		}

        /// <summary>
        /// Calls the FastDownward planner on the compiled plan recognition domain and problem file.
        /// </summary>
		private void _FastDownwardPlan() {
			print("_FastDownwardPlan() has been invoked.");

			// Start Fast Downward's batch file.
			string planBatScriptPath = (Parser.GetTopDirectory() + @"planreco.bat").Replace("/", "\\");
			ProcessStartInfo startInfo = new ProcessStartInfo(planBatScriptPath);
			
			// Store the process' arguments.
			print(planBatScriptPath + " " + Parser.GetScriptDirectory() + " " + this.domainName);
			startInfo.Arguments = Parser.GetScriptDirectory() + " " + this.domainName;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			
			// Start the process and wait for it to finish.
			using (Process proc = Process.Start(startInfo)) {
				proc.WaitForExit();
			}
			
			// Erase old data.
			string outputDirectoryPath = (Parser.GetTopDirectory() + @"Benchmarks\"+this.domainName+@"\pr-output").Replace("/", "\\");
			System.IO.File.WriteAllText(outputDirectoryPath+@"\output", string.Empty);
			System.IO.File.WriteAllText(outputDirectoryPath+@"\output.sas", string.Empty);

			// Edit the output plan for reading by the UGME
			string outputPlanDirectory = outputDirectoryPath+@"\sas_plan";
			string[] input = System.IO.File.ReadAllLines(outputPlanDirectory); // get the existing input
			System.IO.File.WriteAllText(outputPlanDirectory, string.Empty); // clear the output plan
			for(int line = 0; line < input.Length; line++) {
				input[line] = input[line].Replace(" ", string.Empty); // get rid of the spaces per line
				System.IO.File.AppendAllText(outputPlanDirectory, input[line]+"\n"); // write the line back to the output plan
			}

			print("_FastDownwardPlan() has finished! Output can be found at: " + outputDirectoryPath);
		}
	}
}