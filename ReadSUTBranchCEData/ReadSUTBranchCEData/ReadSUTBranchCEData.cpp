#include "ReadSUTBranchCEData.h"
#pragma managed  

namespace ReadSUTBranchCEData {
	using namespace System;
	using namespace sutnamespace;

	void readBranch::ReadBranchCLIFunc(array<int>^ inputs, array<int> ^%outputs, int selectSUT) 
	{	

		array<int> ^ces = gcnew array<int>(100);
		for (int i = 0; i < 100; i++)
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
		outputs = ces;
	}
}