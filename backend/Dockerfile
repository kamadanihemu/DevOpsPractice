# -------- Stage 1: build --------
FROM maven:3.9.9-eclipse-temurin-17 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn -B -DskipTests dependency:go-offline
COPY src ./src
RUN mvn -B -DskipTests clean package

# -------- Stage 2: runtime --------
FROM eclipse-temurin:17-jre
WORKDIR /app
RUN useradd -m appuser
USER appuser
COPY --from=build /app/target/*.jar /app/app.jar
EXPOSE 8080
ENTRYPOINT ["java","-jar","/app/app.jar"]
