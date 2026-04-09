<script setup lang="ts">
import { ref } from 'vue';
import { ValidationService } from '@/services/ValidationService';
import type { IValidationResponse } from '@/types/IValidationResponse';

const validationService = new ValidationService();

const selectedFile = ref<File | null>(null);
const isLoading = ref(false);
const result = ref<IValidationResponse | null>(null);
const errors = ref<string[]>([]);

const onFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;

  const file = input.files[0];

  if (!file) {
    errors.value = ['Faili üleslaadimine ebaõnnestus'];
    selectedFile.value = null;
    return;
  }

  if (!file.name.endsWith('.docx')) {
    errors.value = ['Palun laadige üles DOCX fail'];
    selectedFile.value = null;
    return;
  }

  selectedFile.value = file;
  errors.value = [];
  result.value = null;
};

const validate = async () => {
  if (!selectedFile.value) return;

  isLoading.value = true;
  errors.value = [];
  result.value = null;

  const formData = new FormData();
  formData.append('file', selectedFile.value);

  const response = await validationService.validateAsync(formData);

  if (response.errors) {
    errors.value = response.errors;
  } else if (response.data) {
    result.value = response.data;
  }

  isLoading.value = false;
};
</script>

<template>
  <div>
    <h1>Lõputöö valideerimine</h1>

    <input type="file" accept=".docx" @change="onFileChange" />

    <div v-if="selectedFile">
      <p>Valitud fail: {{ selectedFile.name }}</p>
      <button @click="validate" :disabled="isLoading">
        {{ isLoading ? 'Valideerimine...' : 'Valideeri' }}
      </button>
    </div>

    <div v-if="errors.length > 0">
      <p v-for="error in errors" :key="error">{{ error }}</p>
    </div>

    <div v-if="result">
      <p>{{ result.status }}</p>
      <p>{{ result.fileName }}</p>
    </div>
  </div>
</template>
