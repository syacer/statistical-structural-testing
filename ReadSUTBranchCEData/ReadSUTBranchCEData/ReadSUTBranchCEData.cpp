#include "ReadSUTBranchCEData.h"
#pragma managed  

namespace ReadSUTBranchCEData {
	using namespace System;
	using namespace sutnamespace;

	void readBranch::ReadBranchCLIFunc(array<int>^ inputs, array<int> ^%outputs, int selectSUT) 
	{	
		const int arraySize = 200;
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
			mySUT.BestMove(inputs[0], inputs[1], pp);
		}
		else if (selectSUT == 4)
		{
			nsichneuSUT mySUT;
			mySUT.Nsichneu(inputs[0],inputs[1],inputs[2],
				inputs[3], inputs[4], inputs[5], 
				inputs[6], inputs[7], inputs[8], 
				inputs[9], inputs[10], inputs[11], 
				inputs[12], inputs[13], pp);
		}
		outputs = ces;
	}
}