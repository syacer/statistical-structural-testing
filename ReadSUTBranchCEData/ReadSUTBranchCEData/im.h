 namespace sutnamespace
{
	class imatrix
	{
	  public:
		imatrix();
		~imatrix();
		int inverseMatrix(
			double x1, double x2, double x3, double x4,
			double x5, double x6, double x7, double x8,
			double x9, int* countarray);
	  private:
		int minver(int row, int col, double eps, int* countarray);
		int  mmul(int  row_a, int col_a, int row_b, int col_b, int* countarray);
		double minver_fabs(double n, int* countarray);
		int minverse(int* countarray);
		double  a[3][3];
		double  b[3][3], c[3][3], aa[3][3], a_i[3][3], e[3][3], det;
	};
}