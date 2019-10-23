using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorPIDValueLogger : VectorPid
{
	private string fileName;

	List<Vector3> errorHistory;
	List<Vector3> derivHistory;
	List<Vector3> integHistory;
	List<Vector3> targetHistory;
	List<Vector3> currentHistory;

	Vector3 test1 = Vector3.zero;
	Vector3 test2;
	Vector3 test3;

	int counter = 0;

	public VectorPIDValueLogger(float pFactor, float iFactor, float dFactor, string name) : base(pFactor, iFactor, dFactor)
	{
		this.fileName = name;

		errorHistory = new List<Vector3>();
		derivHistory = new List<Vector3>();
		integHistory = new List<Vector3>();
		targetHistory = new List<Vector3>();
		currentHistory = new List<Vector3>();
	}

	override public Vector3 UpdateNormalPid(Vector3 currentError, float timeFrame, Vector3 target, Vector3 current)
	{
		currentError -= test1;
		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;

		//log the values:
		if(counter > 300)
		{
			errorHistory.Add(currentError);
			derivHistory.Add(deriv);
			integHistory.Add(integral);
			targetHistory.Add(target - test1 -test2);
			currentHistory.Add(current - test2);
		}
		if (counter == 299)
		{
			//eliminate the stupid offset by this;
			test1 = currentError;
			test2 = current;
		}
		counter++;

		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;
	}

	override public Vector3 UpdateModifiedPid(Vector3 currentError, float timeFrame)
	{
		//reduce integrall with time to eliminate huge error when walking moving against walls, ez;
		//only neccesarry for rotations since the axis can change
		integral *= 0.99f;

		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;

		//log the values:
		if (counter > 300)
		{
			errorHistory.Add(currentError);
			derivHistory.Add(deriv);
			integHistory.Add(integral);
		}
		counter++;

		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;
	}


	//some debug stuff because unity loves to only print 1 float digit
	public string printVector(Vector3 vec)
	{
		return ("(" + vec.x.ToString("n4") + " " + vec.y.ToString("n4") + " " + vec.z.ToString("n4") + ")");
	}

	/// <summary>
	/// writes down all the plotting stuff at once at the end so the gameplay itself is not interuppted by stings.
	/// </summary>
	public override void appExit()
	{
		if(counter <= 300)
		{
			return;
		}
		var lines = new string[errorHistory.Count];
		//P value (error)----------------------------------------------
		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + errorHistory[i].x.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "PX.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + errorHistory[i].y.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "PY.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + errorHistory[i].z.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "PZ.txt", lines);
		//I value -------------------------------------------
		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + integHistory[i].x.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "IX.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + integHistory[i].y.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "IY.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + integHistory[i].z.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "IZ.txt", lines);
		//D value --------------------------------------------------------
		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + derivHistory[i].x.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "DX.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + derivHistory[i].y.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "DY.txt", lines);

		for (int i = 0; i < errorHistory.Count; i++)
		{
			lines[i] = i.ToString() + " " + derivHistory[i].z.ToString("n4");
		}
		System.IO.File.WriteAllLines("Data\\" + fileName + "DZ.txt", lines);

		if(currentHistory.Count >10)
		{
			// target value --------------------------------------------------------
			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + targetHistory[i].x.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "TargetX.txt", lines);

			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + targetHistory[i].y.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "TargetY.txt", lines);

			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + targetHistory[i].z.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "TargetZ.txt", lines);

			// current value --------------------------------------------------------
			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + currentHistory[i].x.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "CurrentX.txt", lines);

			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + currentHistory[i].y.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "CurrentY.txt", lines);

			for (int i = 0; i < errorHistory.Count; i++)
			{
				lines[i] = i.ToString() + " " + currentHistory[i].z.ToString("n4");
			}
			System.IO.File.WriteAllLines("Data\\" + fileName + "CurrentZ.txt", lines);
		}

	}
}