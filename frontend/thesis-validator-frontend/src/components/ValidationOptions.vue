<script setup lang="ts">
import type { ITemplateDto } from '@/types/ITemplateDto';
import type { IValidationOptions } from '@/types/IValidationOptions';
import { ref } from 'vue'

const props = defineProps<{
  templates: ITemplateDto[]
}>()

const emit = defineEmits<{
  optionsChanged: [options: IValidationOptions]
}>()

const options = ref<IValidationOptions>({
  templateId: 'taltech-it',
  thesisType: 'bachelor',
  language: 'et',
  curriculumLanguage: 'et',
  foreignTitle: ''
})

const onChanged = () => {
  emit('optionsChanged', { ...options.value })
}
</script>

<template>
  <div class="bg-white border border-gray-200 rounded-xl p-6 space-y-4">
    <h2 class="font-medium text-gray-900">Valideerimise seaded</h2>

    <div class="grid grid-cols-2 gap-4">
      <div class="space-y-1">
        <label class="text-sm text-gray-600">Valideerimismall</label>
        <select v-model="options.templateId" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option v-for="t in props.templates" :key="t.templateId" :value="t.templateId">
            {{ t.name }} ({{ t.version }})
          </option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Töö tüüp</label>
        <select v-model="options.thesisType" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="bachelor">Bakalaureusetöö</option>
          <option value="master">Magistritöö</option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Töö keel</label>
        <select v-model="options.language" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="et">Eesti keel</option>
        </select>
      </div>

      <div class="space-y-1">
        <label class="text-sm text-gray-600">Õppekava keel</label>
        <select v-model="options.curriculumLanguage" @change="onChanged"
          class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 bg-white cursor-pointer">
          <option value="et">Eestikeelne õppekava</option>
        </select>
      </div>
    </div>
  </div>
</template>
