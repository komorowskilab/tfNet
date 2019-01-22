args <- commandArgs(trailingOnly = TRUE)
file <- "null"
numOfPeaks <- 0
output <- "null"
filterMethod <- "null"
filterValue <- 0.0
graphTitle <- "null"
typeOfAnalysis <- "null"
if(length(args) < 10 | length(args)%%2 != 0) {
  stop("tfNet - the number of the provided arguments is incorrect")
}
for(i in seq(1,length(args),by=2)) {
  switch (args[i],
          "-i" = file <- args[i + 1], #input
          "-l" = numOfPeaks <- as.numeric(args[i + 1]), #total number of peaks
          "-o" = output <- args[i + 1], #output
          "-q" = list(filterMethod <- substr(args[i], 2, 2), filterValue <- as.numeric(args[i + 1])), #type of filtering - qValue
          "-p" = list(filterMethod <- substr(args[i], 2, 2), filterValue <- as.numeric(args[i + 1])), #type of filtering - pValue
          "-b" = list(filterMethod <- substr(args[i], 2, 2), filterValue <- as.numeric(args[i + 1])), #type of filtering - bonferroni correction
          "-n" = filterMethod <- substr(args[i], 2, 2), 
          "-t" = graphTitle <- args[i + 1],
          "-a" = typeOfAnalysis <- args[i + 1]
  )
}
if (graphTitle == "null")
  graphTitle <- "tfNetDefault"

#from http://stackoverflow.com/questions/9341635/how-can-i-check-for-installed-r-packages-before-running-install-packages
is.installed <- function(mypkg) is.element(mypkg, installed.packages()[,1])

packs <- c("qvalue", "igraph", "gplots")
for(i in seq(1, length(packs))){
  if(is.installed(packs[i]) == FALSE){
    stop(paste("tfNet - the package", packs[i], "is not installed in R"))
  }
}

#prepare for statistics
data <- read.csv(file, header = FALSE, sep = ",")
numbers <- as.matrix(data[,3:5])
products <- as.numeric(numbers[,2])*as.numeric(numbers[,3])
N <- (numOfPeaks * (numOfPeaks - 1)) / 2
m <- length(data[,1])
mMinus1 <- numbers[,1] - 1
M <- sum(numbers[,1])
falseVector <- rep(FALSE, m)
pValues <- 0
if(typeOfAnalysis == "h"){
  pValues <- mapply(phyper, mMinus1, products, rep(N, m) - products, rep(M, m), falseVector, falseVector)
} else if(typeOfAnalysis == "b"){
  probability <- products / N
  pValues <- mapply(pbinom, mMinus1, rep(M, m), probability, falseVector, falseVector)
}

library(qvalue)
qobj <- qvalue(pValues)

bonferroniCorrectedPValues <- length(pValues) * pValues

pValueNew <- as.data.frame(pValues)
qValueNew <- as.data.frame(qobj$qv)
bonferroniCorrectedPValuesNew <- as.data.frame(bonferroniCorrectedPValues)

returnable <- cbind(data,pValueNew,qValueNew,bonferroniCorrectedPValuesNew)

threshold <- as.numeric(filterValue)
if(filterMethod == "p") {
  selection <- pValues < threshold
  fileteredReturnable <- returnable[selection,]
} else if(filterMethod == "q") {
  selection <- qobj$qv < threshold
  fileteredReturnable <- returnable[selection,]
} else if(filterMethod == "b") {
  selection <- bonferroniCorrectedPValues < threshold
  fileteredReturnable <- returnable[selection,]
}

logarithmQ <- -log(fileteredReturnable[,7], base = 10)
if(("Inf" %in% logarithmQ) == TRUE) {
  logarithmQ[(logarithmQ == "Inf")] <- max(logarithmQ[(logarithmQ != "Inf")])
}
fileteredReturnable <- cbind(fileteredReturnable, logarithmQ)
colnames(fileteredReturnable) <- c("tfA", "tfB", "both", "tfA_occs", "tfB_occs", "pValue", "qValue", "bonferroni", "minus_log10")
write.table(fileteredReturnable,file = paste(output, "_filtered.csv", sep=""), append = FALSE, quote = FALSE, sep = ",",
            eol = "\n", na = "NA", dec = ".", row.names = FALSE, col.names = TRUE, qmethod = c("escape", "double"),
            fileEncoding = "")

names <- unique(c(as.vector(fileteredReturnable[,1]),as.vector(fileteredReturnable[,2])))

if (length(names) == 0){
  stop("no significant interactions detected in this network")
}

adjMatrix <- mat.or.vec(length(names), length(names))
rownames(adjMatrix) <- names
colnames(adjMatrix) <- names

for(j in 1:length(logarithmQ)) {
  adjMatrix[as.vector(fileteredReturnable[j,1]),as.vector(fileteredReturnable[j,2])] = logarithmQ[j]
  adjMatrix[as.vector(fileteredReturnable[j,2]),as.vector(fileteredReturnable[j,1])] = logarithmQ[j]
}

#library(igraph)
#tfNet <- graph.adjacency(adjMatrix, mode="undirected", weighted=TRUE)
#tfNet <- simplify(tfNet)
#V(tfNet)$label <- V(tfNet)$name
#set.seed(3952)
#V(tfNet)$label.cex[(V(tfNet)$label.cex < 1)] <- 0.8
#V(tfNet)$label.color <- rgb(0, 0, .2, .8)
#V(tfNet)$frame.color <- NA
#E(tfNet)$color[!(E(tfNet)$weight > 5)] <- "#B5B5B5E6"
#E(tfNet)$color[(E(tfNet)$weight > 5)] <- "#CC3333E6"
#theName <- paste0(fileToOutputTo, "_network_filtered.tiff")
#tiff(theName, compression = "lzw", width=800, height=800)
#plot(tfNet, edge.width=E(tfNet)$weight, main=graphTitle)
#dev.off()

library(gplots)
normalizedAdjacencyMatrix <- adjMatrix
palette <- colorRampPalette(c('#E5E5E5','#EE0000'), space="Lab")(256)
#colbr <- c(seq(0,length(palette),1)) # vector of symmetric breaks
#normalizedAdjacencyMatrix[upper.tri(normalizedAdjacencyMatrix, diag=TRUE)] <- NA
normalizedAdjacencyMatrix <- normalizedAdjacencyMatrix[, rev(seq_len(ncol(normalizedAdjacencyMatrix)))]
theName <- paste(output, "_heatmap_filtered.pdf", sep="")
pdf(theName)
heatmap.2(normalizedAdjacencyMatrix, Rowv = NA, Colv = NA, main=graphTitle, scale = "none", col=palette, key=TRUE, symkey=FALSE,
          density.info="none", trace="none", lmat=rbind(c(0,4,2), c(0,1,0), c(0,3,0)), lwid=c(0.3,5.5,0.2), lhei=c(1.0,4.2,0.8),
          dendrogram="none")
dev.off()