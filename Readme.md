#Sorter

Solution contains 2 Projects

- FileCreator
- FileSorter

##LargeFileCreator
Creates a new text file. Line format: Number. word  
For example 1. Apple

###Command line params: 

| Param | Description                                             |
|-------|---------------------------------------------------------|
| -l    | Number of lines                                         |
| -o    | Output file                                             |
| -w    | Use CrypticWizard.RandomWordGenerator to generate words |


Example:
```
-l 10000000 -o "out_10m.txt"
```
##LagreFileSorter
Splits a file into chunks, sorts chunks, them merge them:

| Param | Description                          |
|-------|--------------------------------------|
| -i    | Input file name                      |
| -b    | Read buffer size for input file (MB) |
| -r    | Max rows in one chunk                |
| -c    | Read buffer size for chunks (MB)     |
| -w    | Write buffer size for chunks (MB)    |
| -o    | Output file name                     |
| -m    | Number of chunks to merge in one run |
| -t    | Temp folder for chunks               |

Examle:
```
-i "inp_10m.txt" -o "sorted_10m.txt"
```
