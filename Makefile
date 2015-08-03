DIST_NAME=Konstruktor_${REVISION}
DIST_SRC=${DIST_NAME}_src
DIST_BIN=${DIST_NAME}_bin

MSB=msbuild.exe /verbosity:m
DIST_FILES=Release/Konstruktor.dll Release/Konstruktor.pdb

.PHONY: package
package: build packagesource packagebinary

.PHONY: packagesource
packagesource: 
	rm -rf /tmp/${DIST_SRC}
	svn export . /tmp/${DIST_SRC}
	cd /tmp && rm -f ${DIST_SRC}.zip && zip -r ${DIST_SRC}.zip ${DIST_SRC}

.PHONY: build
build:
	${MSB} Konstruktor.sln /p:Configuration=Release /target:Konstruktor:Rebuild	

.PHONY: packagebinary
packagebinary:
	rm -rf /tmp/${DIST_BIN}
	mkdir /tmp/${DIST_BIN}
	cp ${DIST_FILES} /tmp/${DIST_BIN}
	cd /tmp && rm -f ${DIST_BIN}.zip && zip -r ${DIST_BIN}.zip ${DIST_BIN}

