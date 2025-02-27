# Use a lightweight Node.js image
FROM node:alpine

# Set working directory
WORKDIR /app

# Copy package.json and install dependencies
COPY package.json package-lock.json ./
RUN npm ci

# Copy the entire app (after dependencies are installed)
COPY . .

# Expose the Vite development server port
EXPOSE 5173

# Start Vite in development mode
CMD ["npm", "run", "dev"]
