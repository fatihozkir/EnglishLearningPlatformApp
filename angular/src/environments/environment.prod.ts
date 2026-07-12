import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44365/',
  redirectUri: baseUrl,
  clientId: 'EnglishLearningPlatformApp_App',
  responseType: 'code',
  scope: 'offline_access EnglishLearningPlatformApp',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'EnglishLearningPlatformApp',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44365',
      rootNamespace: 'EnglishLearningPlatformApp',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
