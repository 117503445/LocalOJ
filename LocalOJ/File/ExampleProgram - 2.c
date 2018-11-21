#include <stdio.h>
#include <string.h>
int main()
{
	char a[100];
	gets(a);
	int b, c;
	scanf("%d%d", &b, &c);
	printf("I received <%s> ,the length is %d\n", a, strlen(a));
	printf("%d", b + c);
	return 0;
}
