FROM node:22-alpine AS build

WORKDIR /app
RUN corepack enable && corepack prepare pnpm@11.7.0 --activate
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml /app/

RUN pnpm install --frozen-lockfile
COPY . /app

RUN CI=true pnpm run build:prod

FROM nginx:alpine
COPY dynamic-env.json /usr/share/nginx/html
COPY nginx.conf  /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist/EnglishLearningPlatformApp/browser /usr/share/nginx/html
