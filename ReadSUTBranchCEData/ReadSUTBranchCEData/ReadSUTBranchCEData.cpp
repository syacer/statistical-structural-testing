#include "Stdafx.h"
#include "ReadSUTBranchCEData.h"
#pragma managed  

namespace ReadSUTBranchCEData {
	using namespace System;
	using namespace sutnamespace;

	void readBranch::ReadBranchCLIFunc(array<int>^ inputs, array<int> ^%outputs, int selectSUT) 
	{	
		const int arraySize = 600;
		array<int> ^ces = gcnew array<int>(arraySize);
		for (int i = 0; i < arraySize; i++)
		{
			ces[i] = 0;
		}
		pin_ptr<int> pp = &ces[0];
		if (selectSUT == 0)
		{
			TriangleSUT mySUT;
			mySUT.triang(inputs[0], inputs[1], inputs[2], pp);
		}
		else if (selectSUT == 1)
		{
			GcdSUT mySUT;
			mySUT.gcd(inputs[0], inputs[1],pp);
		}
		else if (selectSUT == 2)
		{
			CalDaySUT mySUT;
			mySUT.CalDay(inputs[0], inputs[1], inputs[2],pp);
		}
		else if (selectSUT == 3)
		{
			BestMoveSUT mySUT;
			int ret = mySUT.BestMove(inputs[0], inputs[1], pp);
			//Console::WriteLine(ret);
		}
		else if (selectSUT == 4)
		{
			nsichneuSUT mySUT;
			int ret = mySUT.Nsichneu(inputs[0],inputs[1],inputs[2],
				inputs[3], inputs[4], inputs[5], 
				inputs[6], inputs[7], inputs[8], 
				inputs[9], inputs[10], inputs[11], 
				inputs[12], inputs[13], pp);
		}
		else if (selectSUT == 5)
		{
			bs mySUT;
			int ret = mySUT.binarySearch(inputs[0], pp);
		}
		else if (selectSUT == 6)
		{
			select mySUT;
			int ret = mySUT.Select(inputs[0],inputs[1],pp);
		}
		else if (selectSUT == 7)
		{
			qsortSUT mySUT;
			
			int ret = mySUT.quickSort((float)inputs[0], (float)inputs[1], (float)inputs[2],
				(float)inputs[3], (float)inputs[4], (float)inputs[5],
				(float)inputs[6], (float)inputs[7], (float)inputs[8],
				(float)inputs[9], (float)inputs[10], (float)inputs[11],
				(float)inputs[12], (float)inputs[13],
				(float)inputs[14], (float)inputs[15], (float)inputs[16],
				(float)inputs[17], (float)inputs[18], (float)inputs[19],
				pp);
		}
		else if (selectSUT == 8)
		{
			imatrix mySUT;

			int ret = mySUT.inverseMatrix((double)inputs[0], (double)inputs[1], (double)inputs[2],
				(double)inputs[3], (double)inputs[4], (double)inputs[5],
				(double)inputs[6], (double)inputs[7], (double)inputs[8],
				pp);
		}
		try
		{
			outputs = ces;
		}
		catch (int ex)
		{
			Console::WriteLine(ex);
		}
	}
}