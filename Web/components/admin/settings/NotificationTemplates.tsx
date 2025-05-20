import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Button } from '../../common/Button/Button';
import { Modal } from '../../common/Modal/Modal';
import { Toast } from '../../common/Toast/Toast';
import { getNotificationTemplates, createNotificationTemplate, updateNotificationTemplate, deleteNotificationTemplate } from '../../../services/notificationService';

interface NotificationTemplate {
  id: string;
  title: string;
  type: string;
  content: string;
  language: string;
}

export const NotificationTemplates: React.FC = () => {
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [loading, setLoading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<NotificationTemplate | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const { register, handleSubmit, reset } = useForm<NotificationTemplate>();

  const fetchTemplates = async () => {
    setLoading(true);
    try {
      const data = await getNotificationTemplates();
      setTemplates(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates();
  }, []);

  const onSubmit = async (values: NotificationTemplate) => {
    setLoading(true);
    try {
      if (editing) {
        await updateNotificationTemplate(editing.id, values);
        setToast('Template updated');
      } else {
        await createNotificationTemplate(values);
        setToast('Template created');
      }
      setShowModal(false);
      fetchTemplates();
    } finally {
      setLoading(false);
      reset();
      setEditing(null);
    }
  };

  const handleEdit = (template: NotificationTemplate) => {
    setEditing(template);
    reset(template);
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    setLoading(true);
    try {
      await deleteNotificationTemplate(id);
      setToast('Template deleted');
      fetchTemplates();
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-bold">Notification Templates</h2>
        <Button onClick={() => { setShowModal(true); setEditing(null); reset(); }}>Add Template</Button>
      </div>
      {loading && <div>Loading...</div>}
      <table className="w-full border">
        <thead>
          <tr>
            <th>Title</th>
            <th>Type</th>
            <th>Language</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {templates.map(t => (
            <tr key={t.id} className="border-t">
              <td>{t.title}</td>
              <td>{t.type}</td>
              <td>{t.language}</td>
              <td>
                <Button size="sm" onClick={() => handleEdit(t)}>Edit</Button>
                <Button size="sm" variant="danger" onClick={() => handleDelete(t.id)}>Delete</Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <Modal open={showModal} onClose={() => setShowModal(false)}>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <input {...register('title', { required: true })} placeholder="Title" className="input" />
          <input {...register('type', { required: true })} placeholder="Type" className="input" />
          <select {...register('language', { required: true })} className="input">
            <option value="en">English</option>
            <option value="sq">Albanian</option>
          </select>
          <textarea {...register('content', { required: true })} placeholder="Content" className="input" />
          <div className="flex gap-2">
            <Button type="submit">{editing ? 'Update' : 'Create'}</Button>
            <Button type="button" variant="secondary" onClick={() => setShowModal(false)}>Cancel</Button>
          </div>
        </form>
      </Modal>
      {toast && <Toast message={toast} onClose={() => setToast(null)} />}
    </div>
  );
};
