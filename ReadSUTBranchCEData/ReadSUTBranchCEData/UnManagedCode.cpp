// PassArray1.cpp  
// compile with: /clr  
#ifndef _CRT_RAND_S  
#define _CRT_RAND_S  
#endif  
#include "UnManagedCode.h"
namespace sutnamespace {
#pragma unmanaged  
	using namespace std;
	void TakesAnArray(int* a, int c) {
		cout << "(unmanaged) array received:\n";
		for (int i = 0; i<c; i++)
			cout << "a[" << i << "] = " << a[i] << "\n";

		unsigned int number;
		errno_t err;

		cout << "(unmanaged) modifying array contents...\n";
		for (int i = 0; i<c; i++) {
			err = rand_s(&number);
			if (err == 0)
				a[i] = number % 100;
		}
	}

}
